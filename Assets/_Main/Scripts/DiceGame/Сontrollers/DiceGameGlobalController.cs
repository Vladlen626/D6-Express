using System.Collections.Generic;
using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;
using PlatformCore.Services.Audio;
using PlatformCore.Services.Factory;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class DiceGameGlobalController : IBaseController, IActivatable
	{
		private readonly DiceGameModel diceGameModel;
		private readonly PlayerModel playerModel;
		private readonly LevelModel levelModel;

		private readonly ICameraShakeService cameraShakeService;
		private readonly IObjectFactory objectFactory;
		private readonly ILoggerService loggerService;
		private readonly IAudioService audioService;
		private readonly LifecycleService lifecycleService;
		private readonly ConfigService configService;

		private readonly SceneContext sceneContext;
		private DicePositionsHandler dicePositionsHandler => sceneContext.DiceGameTableView.GameStatePosHandler;
		private DiceTableView diceTableView => sceneContext.DiceGameTableView;
		
		private DiceView[] playerDiceViewsArray;
		private DiceView[] enemyDiceViewsArray;
		private TableModel tableModel;

		private List<IBaseController> gameControllers = new();
		private List<IBaseController> betControllers = new();
		private List<IBaseController> selectionControllers = new();

		private bool gamePreviousStoped = false;
		public DiceGameGlobalController(DiceGameModel diceGameModel, PlayerModel playerModel, SceneContext sceneContext,
			ServiceLocator serviceLocator, LevelModel levelModel, ConfigService configService)
		{
			this.diceGameModel = diceGameModel;
			this.playerModel = playerModel;
			this.levelModel = levelModel;
			this.sceneContext = sceneContext;
			this.configService = configService;
			lifecycleService = serviceLocator.Get<LifecycleService>();
			objectFactory = serviceLocator.Get<IObjectFactory>();
			loggerService = serviceLocator.Get<ILoggerService>();
			cameraShakeService = serviceLocator.Get<ICameraShakeService>();
			audioService = serviceLocator.Get<IAudioService>();
		}

		public void Activate()
		{
			playerModel.PlayerStateModel.StateAdded += OnCharacterStateAddedHandler;
			playerModel.PlayerStateModel.StateRemoved += OnCharacterStateRemovedHandler;
			diceGameModel.OnDiceGameStateChanged += OnDiceGameStateChangedHandler;
			diceGameModel.OnGameConditionPassed += OnGameConditionPassedHandler;
			diceGameModel.OnGameConditionFailed += OnGameConditionFailedHandler;
			OnDiceGameStateChangedHandler();
		}

		public void Deactivate()
		{
			playerModel.PlayerStateModel.StateAdded -= OnCharacterStateAddedHandler;
			playerModel.PlayerStateModel.StateRemoved -= OnCharacterStateRemovedHandler;
			diceGameModel.OnDiceGameStateChanged -= OnDiceGameStateChangedHandler;
			diceGameModel.OnGameConditionPassed -= OnGameConditionPassedHandler;
			diceGameModel.OnGameConditionFailed -= OnGameConditionFailedHandler;
		}

		private void OnGameConditionPassedHandler()
		{
			playerModel.InventoryModel.GiveCash(diceGameModel.BetSize * 2);
			StopDiceGame();
		}

		private void OnGameConditionFailedHandler()
		{
			StopDiceGame();
		}

		private void OnDiceGameStateChangedHandler()
		{
			sceneContext.DiceGameTableView.SwitchGameStateView(diceGameModel.DiceGameState);
		}

		private void OnCharacterStateAddedHandler(CharacterState state)
		{
			if (state == CharacterState.DICE_GAME)
			{
				StartDiceGame().Forget();
			}
		}

		// ReSharper disable Unity.PerformanceAnalysis
		private void OnCharacterStateRemovedHandler(CharacterState state)
		{
			if (state == CharacterState.DICE_GAME)
			{
				StopDiceGame();
			}
		}

		private async UniTask StartDiceGame()
		{
			gamePreviousStoped = false;
			diceTableView.EnableCamera();
			var diceGameConfig =
				await configService.GetFirstOrDefaultAsync<DiceGameConfig>(ResourcePaths.Json.dice_game_rules);

			int maxBetSize = playerModel.InventoryModel.CashCount;
			
			diceGameModel.Setup(diceGameConfig, maxBetSize);
			diceTableView.SwitchTurn(diceGameModel.IsPlayerTurn);
			tableModel = new TableModel(dicePositionsHandler.DicePositions, dicePositionsHandler.BankedPositions);

			await SelectionProcess();
			await BetProcess();
			await SetupEnemyDiceList();

			var processController = new DiceGameProcessController(tableModel, sceneContext.DiceGameTableView,
				loggerService, diceGameModel, cameraShakeService, audioService);

			gameControllers.AddRange(new IBaseController[]
			{
				processController,
				new EnemyTurnController(processController, diceGameModel, tableModel),
				new DiceGameScoreViewController(tableModel, sceneContext.DiceGameTableView, diceGameModel),
				new DiceGameResultController(diceGameModel, tableModel)
			});

			foreach (var controller in gameControllers)
			{
				await lifecycleService.RegisterAsync(controller);
			}
		}

		private async UniTask SelectionProcess()
		{
			diceGameModel.ChangeDiceGameState(DiceGameState.SELECT_DICE);
			
			var selectionController = new DiceSelectionController(
				playerModel.InventoryModel, sceneContext.DiceGameTableView,
				objectFactory, configService, diceGameModel);
			
			selectionControllers.Add(selectionController);

			await lifecycleService.RegisterAsync(selectionController);
			await selectionController.WaitSelection();

			var selectedModels = diceGameModel.PlayerDiceModelList;
			playerDiceViewsArray = new DiceView[selectedModels.Count];

			for (int i = 0; i < selectedModels.Count; i++)
			{
				var model = selectedModels[i];
				var view = diceGameModel.ScreenDiceDict[model];
				var gamePos = dicePositionsHandler.DicePositions[i];

				view.transform.SetParent(gamePos);
				view.MoveToPosition(gamePos.position);
				model.SetCurrentPosition(gamePos);
				playerDiceViewsArray[i] = view;
				gameControllers.Add(new DiceController(model, view, tableModel, audioService));
			}

			ClenUpSelectionControllers();
		}
		
		private async UniTask SetupEnemyDiceList()
		{
			var config = await configService.GetFirstOrDefaultAsync<DiceConfig>(ResourcePaths.Json.dice_game_rules);
			for (int i = 0; i < 6; i++)
			{
				var startPos = diceTableView.GameStatePosHandler.BankedPositions[i];
				DiceView view = await objectFactory.CreateAsync<DiceView>(
					ResourcePaths.Items.DicePrefab, Vector3.zero, Quaternion.identity);

				view.Initialize(config.id, false);
				view.transform.SetParent(startPos);
				view.Hide();

				DiceModel model = new DiceModel(config);
				model.SetCurrentPosition(startPos);
				diceGameModel.EnemyDiceModelList.Add(model);
				diceGameModel.AddDiceOnScreen(model, view);
			}

			var enemyModels = diceGameModel.EnemyDiceModelList;
			enemyDiceViewsArray = new DiceView[enemyModels.Count];

			for (int i = 0; i < enemyModels.Count; i++)
			{
				var model = enemyModels[i];
				var view = diceGameModel.ScreenDiceDict[model];
				var gamePos = dicePositionsHandler.DicePositions[i];

				view.transform.SetParent(gamePos);
				view.MoveToPosition(gamePos.position);
				model.SetCurrentPosition(gamePos);
				enemyDiceViewsArray[i] = view;
				gameControllers.Add(new DiceController(model, view, tableModel, audioService));
			}
		}

		private async UniTask BetProcess()
		{
			diceGameModel.ChangeDiceGameState(DiceGameState.BET);

			betControllers.AddRange(DiceFactory.GetDiceGameBetControllers(sceneContext, diceGameModel));
			foreach (var controller in betControllers)
			{
				await lifecycleService.RegisterAsync(controller);
			}

			await UniTask.WaitUntil(() => diceGameModel.DiceGameState != DiceGameState.BET);

			if (diceGameModel.DiceGameState == DiceGameState.GAME)
			{
				playerModel.InventoryModel.TakeCash(diceGameModel.BetSize);
			}

			ClenUpBetControllers();
		}

		// ReSharper disable Unity.PerformanceAnalysis
		private void StopDiceGame()
		{
			if (gamePreviousStoped)
			{
				return;
			}
			
			diceTableView.DisableCamera();
			if (!levelModel.IsLevelFinished && diceGameModel.IsDiceGameStarted)
			{
				levelModel.IncrementTicks();
			}

			diceGameModel.ChangeDiceGameState(DiceGameState.DEFAULT);
			ResetModels();
			ClenUpSelectionControllers();
			CleanUpMainGameControllers();
			ClenUpBetControllers();
			gamePreviousStoped = true;
		}

		private void ClenUpBetControllers()
		{
			foreach (var controller in betControllers)
			{
				lifecycleService.Unregister(controller);
			}

			betControllers.Clear();
		}
		
		private void ClenUpSelectionControllers()
		{
			foreach (var controller in selectionControllers)
			{
				lifecycleService.Unregister(controller);
			}

			selectionControllers.Clear();
		}

		private void CleanUpMainGameControllers()
		{
			if (playerDiceViewsArray != null)
			{
				foreach (var dice in playerDiceViewsArray)
				{
					objectFactory.Destroy(dice.gameObject);
				}

				playerDiceViewsArray = null;
			}
			
			if (enemyDiceViewsArray != null)
			{
				foreach (var dice in enemyDiceViewsArray)
				{
					objectFactory.Destroy(dice.gameObject);
				}

				enemyDiceViewsArray = null;
			}

			foreach (var controller in gameControllers)
			{
				lifecycleService.Unregister(controller);
			}

			gameControllers.Clear();
		}

		private void ResetModels()
		{
			foreach (var model in diceGameModel.CurrentDiceModelList)
			{
				var dice = diceGameModel.ScreenDiceDict[model];
				diceGameModel.RemoveDiceOnScreen(model);
				objectFactory.Destroy(dice.gameObject);
			}

			diceGameModel.Reset();
			tableModel.Reset();
		}
	}
}
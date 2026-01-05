using System.Collections.Generic;
using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;
using PlatformCore.Services.Factory;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class DiceGameGlobalController : IBaseController, IActivatable
	{
		private readonly DiceGameModel diceGameModel;
		private readonly PlayerModel playerModel;
		private readonly LevelModel levelModel;

		private readonly IObjectFactory objectFactory;
		private readonly ILoggerService loggerService;
		private readonly LifecycleService lifecycleService;
		private readonly ConfigService configService;

		private readonly SceneContext sceneContext;
		private readonly DicePositionsHandler dicePositionsHandler;

		private readonly List<DiceModel> diceModelsList = new();
		private DiceView[] diceViewsArray;
		private TableModel tableModel;

		private List<IBaseController> gameControllers = new();
		private List<IBaseController> betControllers = new();

		public DiceGameGlobalController(DiceGameModel diceGameModel, PlayerModel playerModel, SceneContext sceneContext,
			ServiceLocator serviceLocator, LevelModel levelModel, ConfigService configService)
		{
			this.diceGameModel = diceGameModel;
			this.playerModel = playerModel;
			this.levelModel = levelModel;
			this.sceneContext = sceneContext;
			this.configService = configService;
			dicePositionsHandler = sceneContext.DiceGameTableView.GameStatePosHandler;
			lifecycleService = serviceLocator.Get<LifecycleService>();
			objectFactory = serviceLocator.Get<IObjectFactory>();
			loggerService = serviceLocator.Get<ILoggerService>();
		}

		public void Activate()
		{
			playerModel.PlayerStateModel.StateAdded  += OnCharacterStateAddedHandler;
			playerModel.PlayerStateModel.StateRemoved  += OnCharacterStateRemovedHandler;
			diceGameModel.OnDiceGameStateChanged += OnDiceGameStateChangedHandler;
			diceGameModel.OnGameConditionPassed += OnGameConditionPassedHandler;
			diceGameModel.OnGameConditionFailed += OnGameConditionFailedHandler;
			OnDiceGameStateChangedHandler();
		}

		public void Deactivate()
		{
			playerModel.PlayerStateModel.StateAdded -= OnCharacterStateAddedHandler;
			playerModel.PlayerStateModel.StateRemoved  -= OnCharacterStateRemovedHandler;
			diceGameModel.OnDiceGameStateChanged -= OnDiceGameStateChangedHandler;
			diceGameModel.OnGameConditionPassed -= OnGameConditionPassedHandler;
			diceGameModel.OnGameConditionFailed -= OnGameConditionFailedHandler;
		}

		private void OnGameConditionPassedHandler()
		{
			playerModel.InventoryModel.GiveCash(diceGameModel.BetSize * 2);
			StopDiceGame();
			loggerService.Log("Ура плюс бабки");
		}
		
		private void OnGameConditionFailedHandler()
		{
			StopDiceGame();
			loggerService.Log("Фак минус бабки");
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
			var diceGameConfig =
				await configService.GetFirstOrDefaultAsync<DiceGameConfig>(ResourcePaths.Json.dice_game_rules);

			int maxBetSize = playerModel.InventoryModel.CashCount;

			diceGameModel.SetMinBetSize(diceGameConfig.min_bet_size);
			diceGameModel.SetMaxBetSize(maxBetSize);
			diceGameModel.SetBetSize((diceGameConfig.min_bet_size + maxBetSize)/2);
			diceGameModel.SetTargetScore(diceGameConfig.target_score);
			diceGameModel.SetMaxTurnCount(diceGameConfig.max_turn_count);
			tableModel = new TableModel(dicePositionsHandler.DicePositions, dicePositionsHandler.BankedPositions);

			await BetProcess();
			await SetupDiceForGame();

			gameControllers.AddRange(DiceFactory.GetDiceGameControllers(sceneContext, loggerService,
				diceGameModel, tableModel, diceModelsList));

			foreach (var controller in gameControllers)
			{
				await lifecycleService.RegisterAsync(controller);
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

			playerModel.InventoryModel.TakeCash(diceGameModel.BetSize);
			ClenUpBetControllers();
		}

		private async UniTask SetupDiceForGame()
		{
			diceViewsArray =
				await DiceFactory.SpawnDiceArrayAsync(objectFactory, dicePositionsHandler.DicePositions);

			var diceConfig = await configService.GetFirstOrDefaultAsync<DiceConfig>(ResourcePaths.Json.dice_types);

			foreach (var diceView in diceViewsArray)
			{
				var model = new DiceModel(diceConfig); 
				var controller = new DiceController(model, diceView, tableModel);
				diceModelsList.Add(model);
				gameControllers.Add(controller);
			}
		}

		private void StopDiceGame()
		{
			if (!levelModel.IsLevelFinished)
			{
				levelModel.IncrementTicks();
			}

			diceGameModel.ChangeDiceGameState(DiceGameState.DEFAULT);
			ResetModels();
			CleanUpMainGameControllers();
			ClenUpBetControllers();
		}

		private void ClenUpBetControllers()
		{
			foreach (var controller in betControllers)
			{
				lifecycleService.Unregister(controller);
			}

			betControllers.Clear();
		}

		private void CleanUpMainGameControllers()
		{
			if (diceViewsArray != null)
			{
				foreach (var dice in diceViewsArray)
				{
					objectFactory.Destroy(dice.gameObject);
				}
				
				diceViewsArray = null;
			}

			foreach (var controller in gameControllers)
			{
				lifecycleService.Unregister(controller);
			}
			gameControllers.Clear();
		}
		
		private void ResetModels()
		{
			diceModelsList.Clear();
			diceGameModel.Reset();
			tableModel.Reset();
		}
	}
}
using System.Collections.Generic;
using _Main.Scripts.Core;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;
using PlatformCore.Services.Audio;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class DiceGameGlobalController : IBaseController, IActivatable
	{
		private readonly DiceGameModel diceGameModel;
		private readonly PlayerModel playerModel;
		private readonly Run run;
		private readonly Notifications notifications;

		private readonly ICameraShakeService cameraShakeService;
		private readonly IObjectFactory objectFactory;
		private readonly ILoggerService loggerService;
		private readonly IAudioService audioService;
		private readonly LifecycleService lifecycleService;
		private readonly ConfigService configService;
		private readonly IUIService uiService;
		private readonly IInputService inputService;
		private readonly DiceScoringService scoringService;

		private readonly SceneContext sceneContext;

		private DiceView[] playerDiceViewsArray;
		private DiceView[] enemyDiceViewsArray;

		private List<IBaseController> persistentControllers = new();
		private List<IBaseController> gameControllers = new();
		private List<IBaseController> betControllers = new();
		private List<IBaseController> selectionControllers = new();
		private readonly List<IBaseController> itemControllers = new();
		private readonly List<DiceItemView> itemViews = new();

		private bool gamePreviousStoped = false;

		private CouplePositionsHandler CouplePositionsHandler => sceneContext.DiceGameTableView.GameStatePosHandler;
		private DiceTableView diceTableView => sceneContext.DiceGameTableView;
		private TableModel tableModel => diceGameModel.tableModel;

		public DiceGameGlobalController(DiceGameModel diceGameModel, PlayerModel playerModel, SceneContext sceneContext,
			ServiceLocator serviceLocator, Run run, ConfigService configService, Notifications notifications)
		{
			this.diceGameModel = diceGameModel;
			this.playerModel = playerModel;
			this.run = run;
			this.notifications = notifications;
			this.sceneContext = sceneContext;
			this.configService = configService;
			lifecycleService = serviceLocator.Get<LifecycleService>();
			objectFactory = serviceLocator.Get<IObjectFactory>();
			loggerService = serviceLocator.Get<ILoggerService>();
			cameraShakeService = serviceLocator.Get<ICameraShakeService>();
			audioService = serviceLocator.Get<IAudioService>();
			uiService = serviceLocator.Get<IUIService>();
			inputService = serviceLocator.Get<IInputService>();
			scoringService = serviceLocator.Get<DiceScoringService>();
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
			inputService.EnableDiceGameInputs();

			await SetupBaseModels();
			await DiceGamePersistentControllers();
			await SetupItemsDisplay();
			await SelectionProcess();

			MoveItemsToGameSlots();

			await BetProcess();
			await SetupEnemyDiceList();

			var processController = new DiceGameProcessController(
				loggerService, diceGameModel, cameraShakeService, audioService, scoringService, run, notifications);

			gameControllers.AddRange(new IBaseController[]
			{
				processController,
				new EnemyTurnController(processController, diceGameModel, scoringService),
				new DiceGameViewController(sceneContext.DiceGameTableView, diceGameModel, cameraShakeService),
				new DiceGameResultController(diceGameModel)
			});
			
			await lifecycleService.RegisterControllersGroupAsync(gameControllers);
		}

		// ReSharper disable Unity.PerformanceAnalysis
		private void StopDiceGame()
		{
			if (gamePreviousStoped)
			{
				return;
			}
			gamePreviousStoped = true;

			inputService.DisableDiceGameInputs();
			
			if (diceGameModel.IsDiceGameStarted)
			{
				run.RequestIncrementTick();
			}

			diceGameModel.ChangeDiceGameState(DiceGameState.DEFAULT);
			ResetModels();
			CleanUpItems();
			ClenUpSelectionControllers();
			CleanUpMainGameControllers();
			ClenUpBetControllers();
			ClenUpPersistentControllers();
		}

		private async UniTask SetupBaseModels()
		{
			var diceGameConfig = await configService.GetFirstOrDefaultAsync<DiceGameConfig>(ResourcePaths.Json.dice_game_rules);
			var newTableModel = new TableModel(CouplePositionsHandler.FirstPosArray, CouplePositionsHandler.SecondPosArray);
			diceGameModel.Setup(diceGameConfig, playerModel.InventoryModel.CashCount, newTableModel);
			// Keep the base cap aligned with available board slots; items can extend beyond this value.
			var baseCap = Mathf.Min(6, CouplePositionsHandler.FirstPosArray.Length, CouplePositionsHandler.SecondPosArray.Length);
			diceGameModel.SetBaseMaxDiceCount(baseCap);
			diceTableView.SwitchTurn(diceGameModel.IsPlayerTurn);
		}
		
		private async UniTask DiceGamePersistentControllers()
		{
			persistentControllers.AddRange(
				new IBaseController[]
				{
				});

			await lifecycleService.RegisterControllersGroupAsync(persistentControllers);
		}


		private async UniTask SelectionProcess()
		{
			diceGameModel.ChangeDiceGameState(DiceGameState.SELECT_DICE);

			var selectionController = new DiceSelectionController(
				playerModel.InventoryModel, sceneContext.DiceGameTableView,
				objectFactory, configService, diceGameModel, audioService);

			selectionControllers.Add(selectionController);

			await lifecycleService.RegisterAsync(selectionController);
			await selectionController.WaitSelection();

			var selectedModels = diceGameModel.PlayerDiceModelList;
			var activeSlots = CouplePositionsHandler.FirstPosArray;
			var playerLimit = Mathf.Min(selectedModels.Count, activeSlots.Length);
			playerDiceViewsArray = new DiceView[playerLimit];

			for (int i = 0; i < playerLimit; i++)
			{
				var model = selectedModels[i];
				var view = diceGameModel.ScreenDiceDict[model];
				var gamePos = activeSlots[i];

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
			var catalog = await configService.GetConfigsAsync<ItemCatalogEntry>(ResourcePaths.Json.items_catalog);
			if (!catalog.TryGetValue("default", out var config) || config.typeEnum != ItemCatalogType.Dice)
			{
				Debug.LogWarning("[DiceGame] Default dice entry not found in catalog.");
				return;
			}
			var bankSlots = diceTableView.GameStatePosHandler.SecondPosArray;
			var activeSlots = CouplePositionsHandler.FirstPosArray;
			// Enemy should not benefit from player dice cap bonuses.
			var enemyLimit = Mathf.Min(diceGameModel.BaseMaxDiceCount, bankSlots.Length, activeSlots.Length);

			for (int i = 0; i < enemyLimit; i++)
			{
				var startPos = bankSlots[i];
				var model = await DiceFactory.SpawnDiceViewAsync(
					objectFactory,
					config,
					Vector3.zero,
					Quaternion.identity,
					startPos,
					false,
					audioService,
					diceGameModel,
					hideOnSpawn: true);
				
				diceGameModel.EnemyDiceModelList.Add(model);
			}

			var enemyModels = diceGameModel.EnemyDiceModelList;
			enemyDiceViewsArray = new DiceView[Mathf.Min(enemyModels.Count, activeSlots.Length)];

			for (int i = 0; i < enemyDiceViewsArray.Length; i++)
			{
				var model = enemyModels[i];
				var view = diceGameModel.ScreenDiceDict[model];
				var gamePos = activeSlots[i];

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

		private void ClenUpPersistentControllers()
		{
			lifecycleService.UnregisterControllersGroup(persistentControllers);
			persistentControllers.Clear();
		}

		private void ClenUpBetControllers()
		{
			lifecycleService.UnregisterControllersGroup(betControllers);
			betControllers.Clear();
		}

		private void ClenUpSelectionControllers()
		{
			lifecycleService.UnregisterControllersGroup(selectionControllers);
			selectionControllers.Clear();
		}

		private void CleanUpMainGameControllers()
		{
			if (playerDiceViewsArray != null)
			{
				foreach (var dice in playerDiceViewsArray)
				{
					if (dice)
					{
						objectFactory.Destroy(dice.gameObject);
					}
				}

				playerDiceViewsArray = null;
			}

			if (enemyDiceViewsArray != null)
			{
				foreach (var dice in enemyDiceViewsArray)
				{
					if (dice)
					{
						objectFactory.Destroy(dice.gameObject);
					}
				}

				enemyDiceViewsArray = null;
			}

			lifecycleService.UnregisterControllersGroup(gameControllers);
			gameControllers.Clear();
		}

		private async UniTask SetupItemsDisplay()
		{
			var items = diceGameModel.ModifierItemsModel.Items;
			if (items.Count == 0)
			{
				return;
			}

			if (!diceTableView.ItemViewPrefab)
			{
				Debug.LogWarning("[DiceGame] ItemViewPrefab is not assigned on DiceTableView. Items will not be spawned.");
				return;
			}

			var slots = diceTableView.ItemSlotsSelection;

			for (int i = 0; i < items.Count; i++)
			{
				var slot = slots != null && i < slots.Length ? slots[i] : null;
				var prefab = (items[i] as IModifierItemViewProvider)?.GetViewPrefab() ?? diceTableView.ItemViewPrefab;
				var view = UnityEngine.Object.Instantiate(
					prefab,
					slot ? slot.position : Vector3.zero,
					slot ? slot.rotation : Quaternion.identity);

				if (slot)
				{
					view.transform.SetParent(slot);
				}

				var controller = new ModifierItemController(items[i], view);
				itemControllers.Add(controller);
				itemViews.Add(view);
				await lifecycleService.RegisterAsync(controller);
			}
		}

		private void MoveItemsToGameSlots()
		{
			if (itemViews.Count == 0)
			{
				return;
			}

			var slots = diceTableView.ItemSlotsGame;
			for (int i = 0; i < itemViews.Count; i++)
			{
				var slot = slots != null && i < slots.Length ? slots[i] : null;
				if (!slot)
				{
					continue;
				}

				var view = itemViews[i];
				view.transform.SetParent(slot);
				view.transform.position = slot.position;
				view.transform.rotation = slot.rotation;
			}
		}

		private void CleanUpItems()
		{
			lifecycleService.UnregisterControllersGroup(itemControllers);
			itemControllers.Clear();

			foreach (var view in itemViews)
			{
				if (view)
				{
					objectFactory.Destroy(view.gameObject);
				}
			}
			itemViews.Clear();
		}

		private void ResetModels()
		{
			foreach (var model in diceGameModel.EnemyDiceModelList)
			{
				var dice = diceGameModel.ScreenDiceDict[model];
				diceGameModel.RemoveDiceOnScreen(model);
				objectFactory.Destroy(dice.gameObject);
			}

			foreach (var model in diceGameModel.PlayerDiceModelList)
			{
				var dice = diceGameModel.ScreenDiceDict[model];
				diceGameModel.RemoveDiceOnScreen(model);
				objectFactory.Destroy(dice.gameObject);
			}

			diceGameModel.Reset();
		}
	}
}

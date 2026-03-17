using System;
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
		private readonly PlayerView playerView;
		private readonly Run run;
		private readonly GlobalNotificationService notificationService;

		private readonly ICameraShakeService cameraShakeService;
		private readonly IObjectFactory objectFactory;
		private readonly ILoggerService loggerService;
		private readonly IAudioService audioService;
		private readonly LifecycleService lifecycleService;
		private readonly ConfigService configService;
		private readonly IResourceService resourceService;
		private readonly IUIService uiService;
		private readonly IInputService inputService;
		private readonly IAsyncAwaiterService awaiterService;
		private readonly ILocalizationService localizationService;
		private readonly DiceGameScenarioSetup scenarioSetup;
		private readonly DiceGameItemsDisplayManager itemsDisplayManager;

		private readonly SceneContext sceneContext;

		private DiceView[] playerDiceViewsArray;
		private DiceView[] enemyDiceViewsArray;
		private EnemyAiScenarioRuntime enemyScenarioRuntime;

		private DicePreGameController dicePreGameController;
		private List<IBaseController> persistentControllers = new();
		private List<IBaseController> gameControllers = new();
		private List<IBaseController> betControllers = new();
		private List<IBaseController> selectionControllers = new();
		private bool gamePreviousStoped = false;

		private CouplePositionsHandler CouplePositionsHandler => sceneContext.DiceGameTableView.GameStatePosHandler;
		private DiceTableView diceTableView => sceneContext.DiceGameTableView;
		private TableModel tableModel => diceGameModel.tableModel;

		public DiceGameGlobalController(DiceGameModel diceGameModel, PlayerModel playerModel, PlayerView playerView, SceneContext sceneContext,
			ServiceLocator serviceLocator, Run run, ConfigService configService, GlobalNotificationService notificationService)
		{
			this.diceGameModel = diceGameModel;
			this.playerModel = playerModel;
			this.playerView = playerView;
			this.run = run;
			this.notificationService = notificationService;
			this.sceneContext = sceneContext;
			this.configService = configService;
			lifecycleService = serviceLocator.Get<LifecycleService>();
			objectFactory = serviceLocator.Get<IObjectFactory>();
			loggerService = serviceLocator.Get<ILoggerService>();
			resourceService = serviceLocator.Get<IResourceService>();
			cameraShakeService = serviceLocator.Get<ICameraShakeService>();
			audioService = serviceLocator.Get<IAudioService>();
			uiService = serviceLocator.Get<IUIService>();
			inputService = serviceLocator.Get<IInputService>();
			awaiterService = serviceLocator.Get<IAsyncAwaiterService>();
			localizationService = serviceLocator.Get<ILocalizationService>();
			scenarioSetup = new DiceGameScenarioSetup(diceGameModel, run, configService, resourceService);
			itemsDisplayManager = new DiceGameItemsDisplayManager(
				diceGameModel,
				sceneContext.DiceGameTableView,
				lifecycleService,
				objectFactory,
				notificationService);
		}

		public void Activate()
		{
			playerModel.PlayerStateModel.StateAdded += OnCharacterStateAddedHandler;
			playerModel.PlayerStateModel.StateRemoved += OnCharacterStateRemovedHandler;
			diceGameModel.OnDiceGameStateChanged += OnDiceGameStateChangedHandler;
			diceGameModel.OnGameConditionPassed += OnGameConditionPassedHandler;
			diceGameModel.OnGameConditionFailed += OnGameConditionFailedHandler;
			OnDiceGameStateChangedHandler();

			dicePreGameController = new DicePreGameController(sceneContext, playerView, playerModel, run);
			lifecycleService.RegisterAsync(dicePreGameController).Forget();
		}

		public void Deactivate()
		{
			lifecycleService.Unregister(dicePreGameController);
			dicePreGameController = null;

			playerModel.PlayerStateModel.StateAdded -= OnCharacterStateAddedHandler;
			playerModel.PlayerStateModel.StateRemoved -= OnCharacterStateRemovedHandler;
			diceGameModel.OnDiceGameStateChanged -= OnDiceGameStateChangedHandler;
			diceGameModel.OnGameConditionPassed -= OnGameConditionPassedHandler;
			diceGameModel.OnGameConditionFailed -= OnGameConditionFailedHandler;
		}

		private void OnGameConditionPassedHandler()
		{
			playerModel.InventoryModel.GiveCash(diceGameModel.CalculateWinPayout());

			playerModel.PlayerStateModel.StateRemoved += OnPostDialogueFinished;
			PostGameDialogue(true);
		}

		private void OnGameConditionFailedHandler()
		{
			playerModel.PlayerStateModel.StateRemoved += OnPostDialogueFinished;
			PostGameDialogue(false);
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

			inputService.OnDiceGameLeave += OnExitRequested;

			if (!await SetupBaseModels())
			{
				return;
			}
			await DiceGamePersistentControllers();
			await itemsDisplayManager.SetupItemsDisplayAsync();
			await SelectionProcess();

			itemsDisplayManager.MoveItemsToGameSlots();

			await BetProcess();
			if (!await SetupEnemyDiceList())
			{
				return;
			}

			var upgradeAwaiter = awaiterService.GetPool("dice.upgrade");
			var tableView = sceneContext.DiceGameTableView;
			var upgradeController = new DiceGameUpgradeController(
				diceGameModel,
				run,
				loggerService,
				upgradeAwaiter,
				objectFactory,
				audioService,
				notificationService,
				localizationService,
				tableView);
			var processController = new DiceGameProcessController(
				loggerService, diceGameModel, cameraShakeService, audioService, run, notificationService, upgradeAwaiter);

			gameControllers.AddRange(new IBaseController[]
			{
				upgradeController,
				processController,
				new DiceGameUpgradeVisualController(uiService, upgradeController, upgradeAwaiter, resourceService, loggerService),
				new EnemyTurnController(processController, diceGameModel, enemyScenarioRuntime),
				new DiceGameViewController(tableView, diceGameModel, cameraShakeService, notificationService),
				new DiceGameResultController(diceGameModel)
			});

			await lifecycleService.RegisterControllersGroupAsync(gameControllers);
			processController.TryStartInitialRoll();
		}

		// ReSharper disable Unity.PerformanceAnalysis
		private void StopDiceGame()
		{
			inputService.OnDiceGameLeave -= OnExitRequested;

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
			itemsDisplayManager.CleanUpItems();
			ClenUpSelectionControllers();
			CleanUpMainGameControllers();
			ClenUpBetControllers();
			ClenUpPersistentControllers();
			RemoveConsumedPlayerModifierItemsFromInventory();
			ResetModels();
		}

		private async UniTask<bool> SetupBaseModels()
		{
			var diceGameConfig = await configService.GetFirstOrDefaultAsync<DiceGameConfig>(ResourcePaths.Json.dice_game_rules);
			if (diceGameConfig == null)
			{
				FailDiceGameSetup("[DiceGame] dice_game_rules config is missing.");
				return false;
			}

			var runConfig = await configService.GetFirstOrDefaultAsync<RunConfig>(ResourcePaths.Json.run_rules);
			if (runConfig == null || runConfig.levels == null || run.Level < 0 || run.Level >= runConfig.levels.Length)
			{
				FailDiceGameSetup("[DiceGame] run_rules config is missing or does not contain current level.");
				return false;
			}

			var levelConfig = runConfig.levels[run.Level];
			if (levelConfig == null)
			{
				FailDiceGameSetup("[DiceGame] run_rules level config is null for current level.");
				return false;
			}

			var day = run.Day + 1;
			var match = run.Tick + 1;
			if (!levelConfig.TryResolveTargetScore(day, match, out var stageTargetScore) || stageTargetScore <= 0)
			{
				FailDiceGameSetup($"[DiceGame] target_score_schedule is missing target for level={run.Level + 1}, day={day}, match={match} in run_rules.");
				return false;
			}

			var newTableModel = new TableModel(CouplePositionsHandler.FirstPosArray, CouplePositionsHandler.SecondPosArray);
			diceGameModel.Setup(diceGameConfig, playerModel.InventoryModel.CashCount, newTableModel);
			diceGameModel.SetTargetScore(stageTargetScore);
			// Keep the base cap aligned with available board slots; items can extend beyond this value.
			var baseCap = DiceGameSetupUtils.CalcBaseCap(
				CouplePositionsHandler.FirstPosArray.Length,
				CouplePositionsHandler.SecondPosArray.Length);
			diceGameModel.SetBaseMaxDiceCount(baseCap);
			diceTableView.SwitchTurn(diceGameModel.IsPlayerTurn);
			if (!await scenarioSetup.SetupEnemyAiScenarioAsync(diceGameConfig))
			{
				return false;
			}
			enemyScenarioRuntime = scenarioSetup.EnemyScenarioRuntime;

			if (!await scenarioSetup.SetupModifiersAsync(diceGameConfig))
			{
				return false;
			}

			return true;
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

		private async UniTask<bool> SetupEnemyDiceList()
		{
			var catalog = await configService.GetConfigsAsync<ItemCatalogEntry>(ResourcePaths.Json.items_catalog);
			var bankSlots = diceTableView.GameStatePosHandler.SecondPosArray;
			var activeSlots = CouplePositionsHandler.FirstPosArray;
			var enemyDiceConfigs = ResolveEnemyDiceConfigs(catalog, bankSlots.Length, activeSlots.Length);
			if (enemyDiceConfigs == null || enemyDiceConfigs.Count == 0)
			{
				FailDiceGameSetup("[DiceGame] Unable to resolve enemy dice setup.");
				return false;
			}

			for (int i = 0; i < enemyDiceConfigs.Count; i++)
			{
				var startPos = bankSlots[i];
				var model = await DiceFactory.SpawnDiceViewAsync(
					objectFactory,
					enemyDiceConfigs[i],
					Vector3.zero,
					Quaternion.identity,
					startPos,
					false,
					audioService,
					diceGameModel,
					hideOnSpawn: true);

				if (model == null)
				{
					FailDiceGameSetup($"[DiceGame] Failed to spawn enemy dice '{enemyDiceConfigs[i].id}'.");
					return false;
				}

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

			return true;
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
			playerDiceViewsArray = null;
			enemyDiceViewsArray = null;

			lifecycleService.UnregisterControllersGroup(gameControllers);
			gameControllers.Clear();
		}

		private void ResetModels()
		{
			enemyScenarioRuntime = null;

			foreach (var model in diceGameModel.EnemyDiceModelList)
			{
				if (diceGameModel.ScreenDiceDict.TryGetValue(model, out var dice))
				{
					diceGameModel.RemoveDiceOnScreen(model);
					if (dice)
					{
						objectFactory.Destroy(dice.gameObject);
					}
				}
			}

			foreach (var model in diceGameModel.PlayerDiceModelList)
			{
				if (diceGameModel.ScreenDiceDict.TryGetValue(model, out var dice))
				{
					diceGameModel.RemoveDiceOnScreen(model);
					if (dice)
					{
						objectFactory.Destroy(dice.gameObject);
					}
				}
			}

			diceGameModel.Reset();
		}

		private void RemoveConsumedPlayerModifierItemsFromInventory()
		{
			var items = diceGameModel.PlayerModifierItemsModel?.Items;
			if (items == null || items.Count == 0)
			{
				return;
			}

			var consumedIds = new List<string>();
			for (int i = 0; i < items.Count; i++)
			{
				var item = items[i];
				(item as IOnMatchFinishedItem)?.OnMatchFinished();

				if (item != null && item.State == DiceItemState.Consumed)
				{
					consumedIds.Add(item.Id);
				}
			}

			for (int i = 0; i < consumedIds.Count; i++)
			{
				playerModel.InventoryModel.RemoveModifierItem(consumedIds[i]);
			}
		}

		private void OnExitRequested()
		{
			if (playerModel.PlayerStateModel.HasState(CharacterState.SPEAKING))
			{
				return;
			}

			// todo: убрать такой способ
			var interactable = sceneContext.DiceGameOpponent.GetComponent<InteractableSpeakable>();
			interactable.SetId(96);
			playerView.Interactor.Interact(interactable);
			interactable.ResetId();
		}

		private List<ItemCatalogEntry> ResolveEnemyDiceConfigs(
			IReadOnlyDictionary<string, ItemCatalogEntry> catalog,
			int bankSlotsCount,
			int activeSlotsCount)
		{
			var maxBySlots = DiceGameSetupUtils.CalcMaxBySlots(bankSlotsCount, activeSlotsCount);
			if (maxBySlots <= 0)
			{
				return new List<ItemCatalogEntry>();
			}

			var scriptedDiceIds = enemyScenarioRuntime?.Scenario?.enemy_setup?.dice_in_hand;
			if (scriptedDiceIds != null && scriptedDiceIds.Length > 0)
			{
				if (!DiceGameSetupUtils.TryResolveScriptedEnemyDiceConfigs(
						catalog,
						scriptedDiceIds,
						maxBySlots,
						out var scriptedConfigs,
						out var error))
				{
					Debug.LogError($"[DiceGame] {error}");
					return null;
				}

				return scriptedConfigs;
			}

			// Enemy should not benefit from player dice cap bonuses.
			var enemyLimit = Mathf.Min(diceGameModel.BaseMaxDiceCount, maxBySlots);
			if (!DiceGameSetupUtils.TryResolveDefaultEnemyDiceConfigs(
					catalog,
					enemyLimit,
					out var defaults,
					out var defaultError))
			{
				Debug.LogWarning($"[DiceGame] {defaultError}");
				return null;
			}

			return defaults;
		}

		private void FailDiceGameSetup(string message)
		{
			Debug.LogError(message);
			diceGameModel.SetConditionFailed();
		}

		private void OnPostDialogueFinished(CharacterState state)
		{
			if (state == CharacterState.SPEAKING)
			{
				playerModel.PlayerStateModel.StateRemoved -= OnPostDialogueFinished;
				StopDiceGame();
			}
		}

		private void PostGameDialogue(bool result)
		{
			var id = result ? 98 : 99;
			// todo: убрать такой способ
			var interactable = sceneContext.DiceGameOpponent.GetComponent<InteractableSpeakable>();
			interactable.SetId(id);
			playerView.Interactor.Interact(interactable);
			interactable.ResetId();
		}
	}
}

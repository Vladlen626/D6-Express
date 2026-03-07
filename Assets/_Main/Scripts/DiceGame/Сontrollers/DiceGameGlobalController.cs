using System;
using System.Collections.Generic;
using _Main.Scripts.Core;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
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

		private readonly SceneContext sceneContext;

		private DiceView[] playerDiceViewsArray;
		private DiceView[] enemyDiceViewsArray;
		private EnemyAiScenarioRuntime enemyScenarioRuntime;

		private List<IBaseController> persistentControllers = new();
		private List<IBaseController> gameControllers = new();
		private List<IBaseController> betControllers = new();
		private List<IBaseController> selectionControllers = new();
		private readonly List<IBaseController> itemControllers = new();
		private readonly List<DiceItemView> itemViews = new();
		private readonly List<IModifier> runtimeGlobalModifiers = new();

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
			
			inputService.OnInteractPressed += OnExitRequested;

			if (!await SetupBaseModels())
			{
				return;
			}
			await DiceGamePersistentControllers();
			await SetupItemsDisplay();
			await SelectionProcess();

			MoveItemsToGameSlots();

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
		}

		// ReSharper disable Unity.PerformanceAnalysis
		private void StopDiceGame()
		{
			inputService.OnInteractPressed -= OnExitRequested;

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
			CleanUpItems();
			ClenUpSelectionControllers();
			CleanUpMainGameControllers();
			ClenUpBetControllers();
			ClenUpPersistentControllers();
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

			var newTableModel = new TableModel(CouplePositionsHandler.FirstPosArray, CouplePositionsHandler.SecondPosArray);
			diceGameModel.Setup(diceGameConfig, playerModel.InventoryModel.CashCount, newTableModel);
			// Keep the base cap aligned with available board slots; items can extend beyond this value.
			var baseCap = Mathf.Min(6, CouplePositionsHandler.FirstPosArray.Length, CouplePositionsHandler.SecondPosArray.Length);
			diceGameModel.SetBaseMaxDiceCount(baseCap);
			diceTableView.SwitchTurn(diceGameModel.IsPlayerTurn);
			if (!await SetupEnemyAiScenarioAsync(diceGameConfig))
			{
				return false;
			}

			return await SetupModifiersAsync(diceGameConfig);
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

		private async UniTask SetupItemsDisplay()
		{
			var items = diceGameModel.PlayerModifierItemsModel.Items;
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

		private async UniTask<bool> SetupEnemyAiScenarioAsync(DiceGameConfig diceGameConfig)
		{
			enemyScenarioRuntime = null;
			var scriptedModeEnabled = string.Equals(
				diceGameConfig.enemy_ai_mode,
				EnemyAiMode.Scripted,
				StringComparison.OrdinalIgnoreCase);

			if (!scriptedModeEnabled)
			{
				return true;
			}

			var scenarioMap = await LoadEnemyScenarioMapAsync();
			if (scenarioMap == null)
			{
				return false;
			}

			var scenarioId = await ResolveScenarioIdAsync(diceGameConfig);
			if (string.IsNullOrWhiteSpace(scenarioId))
			{
				return false;
			}

			if (!scenarioMap.TryGetValue(scenarioId, out var scenario) || scenario == null)
			{
				FailDiceGameSetup($"[DiceGame] Enemy AI scenario '{scenarioId}' not found in map '{ResourcePaths.Json.enemy_ai_scenarios}'.");
				return false;
			}

			if (!scenario.TryValidateStatic(out var validationError))
			{
				FailDiceGameSetup($"[DiceGame] Scenario validation failed: {validationError}");
				return false;
			}

			if (scenario.target_score > 0 && scenario.target_score != diceGameModel.TargetPoints)
			{
				FailDiceGameSetup(
					$"[DiceGame] Scenario target_score ({scenario.target_score}) does not match dice_game_rules target_score ({diceGameModel.TargetPoints}).");
				return false;
			}

			diceGameModel.SetEnemyComboUpgradesEnabled(false);
			enemyScenarioRuntime = new EnemyAiScenarioRuntime(scenario);
			return true;
		}

		private async UniTask<Dictionary<string, EnemyAiScenarioConfig>> LoadEnemyScenarioMapAsync()
		{
			var textAsset = await resourceService.LoadAsync<TextAsset>(ResourcePaths.Json.enemy_ai_scenarios);
			if (!textAsset)
			{
				FailDiceGameSetup($"[DiceGame] Enemy AI scenarios file '{ResourcePaths.Json.enemy_ai_scenarios}' not found.");
				return null;
			}

			Dictionary<string, EnemyAiScenarioConfig> scenarioMap;
			try
			{
				scenarioMap = JsonConvert.DeserializeObject<Dictionary<string, EnemyAiScenarioConfig>>(textAsset.text);
			}
			catch (Exception exception)
			{
				FailDiceGameSetup($"[DiceGame] Failed to parse scenarios map '{ResourcePaths.Json.enemy_ai_scenarios}': {exception.Message}");
				return null;
			}

			if (scenarioMap == null || scenarioMap.Count == 0)
			{
				FailDiceGameSetup($"[DiceGame] Scenarios map '{ResourcePaths.Json.enemy_ai_scenarios}' is empty.");
				return null;
			}

			foreach (var pair in scenarioMap)
			{
				if (string.IsNullOrWhiteSpace(pair.Key))
				{
					FailDiceGameSetup("[DiceGame] Scenarios map contains an empty key.");
					return null;
				}

				if (pair.Value == null)
				{
					FailDiceGameSetup($"[DiceGame] Scenario '{pair.Key}' is null.");
					return null;
				}

				pair.Value.id = pair.Key;
				pair.Value.ParseConfig();
			}

			return scenarioMap;
		}

		private async UniTask<string> ResolveScenarioIdAsync(DiceGameConfig diceGameConfig)
		{
			var scenarioId = diceGameConfig.enemy_ai_scenario_id?.Trim();
			if (!string.IsNullOrWhiteSpace(scenarioId))
			{
				return scenarioId;
			}

			var schedule = await configService.GetFirstOrDefaultAsync<EnemyAiScenarioScheduleConfig>(ResourcePaths.Json.enemy_ai_scenario_schedule);
			if (schedule == null)
			{
				FailDiceGameSetup($"[DiceGame] Enemy AI schedule '{ResourcePaths.Json.enemy_ai_scenario_schedule}' not found and enemy_ai_scenario_id override is empty.");
				return null;
			}

			schedule.ParseConfig();
			if (!schedule.TryValidateStatic(out var scheduleValidationError))
			{
				FailDiceGameSetup($"[DiceGame] Enemy AI schedule validation failed: {scheduleValidationError}");
				return null;
			}

			// Schedule values are 1-based for easier authoring in JSON.
			var level = run.Level + 1;
			var day = run.Day + 1;
			var match = run.Tick + 1;
			if (!schedule.TryResolveScenarioId(level, day, match, out scenarioId))
			{
				FailDiceGameSetup($"[DiceGame] Enemy AI schedule could not resolve scenario for level={level}, day={day}, match={match}.");
				return null;
			}

			return scenarioId;
		}

		private List<ItemCatalogEntry> ResolveEnemyDiceConfigs(
			IReadOnlyDictionary<string, ItemCatalogEntry> catalog,
			int bankSlotsCount,
			int activeSlotsCount)
		{
			var maxBySlots = Mathf.Min(bankSlotsCount, activeSlotsCount);
			if (maxBySlots <= 0)
			{
				return new List<ItemCatalogEntry>();
			}

			var scriptedDiceIds = enemyScenarioRuntime?.Scenario?.enemy_setup?.dice_in_hand;
			if (scriptedDiceIds != null && scriptedDiceIds.Length > 0)
			{
				if (scriptedDiceIds.Length > maxBySlots)
				{
					Debug.LogError(
						$"[DiceGame] Scenario requires {scriptedDiceIds.Length} enemy dice, but only {maxBySlots} slots are available.");
					return null;
				}

				var scriptedConfigs = new List<ItemCatalogEntry>(scriptedDiceIds.Length);
				for (int i = 0; i < scriptedDiceIds.Length; i++)
				{
					var diceId = scriptedDiceIds[i];
					if (!catalog.TryGetValue(diceId, out var diceConfig) || diceConfig.typeEnum != ItemCatalogType.Dice)
					{
						Debug.LogError($"[DiceGame] Scenario dice id '{diceId}' is missing or is not a Dice entry.");
						return null;
					}

					scriptedConfigs.Add(diceConfig);
				}

				return scriptedConfigs;
			}

			if (!catalog.TryGetValue("default", out var defaultConfig) || defaultConfig.typeEnum != ItemCatalogType.Dice)
			{
				Debug.LogWarning("[DiceGame] Default dice entry not found in catalog.");
				return null;
			}

			// Enemy should not benefit from player dice cap bonuses.
			var enemyLimit = Mathf.Min(diceGameModel.BaseMaxDiceCount, maxBySlots);
			var defaults = new List<ItemCatalogEntry>(enemyLimit);
			for (int i = 0; i < enemyLimit; i++)
			{
				defaults.Add(defaultConfig);
			}

			return defaults;
		}

		private async UniTask<bool> SetupModifiersAsync(DiceGameConfig diceGameConfig)
		{
			var modifiersMode = diceGameConfig.modifiers_mode?.Trim();
			if (string.IsNullOrWhiteSpace(modifiersMode))
			{
				modifiersMode = DiceGameModifiersMode.Inventory;
			}

			if (string.Equals(modifiersMode, DiceGameModifiersMode.Inventory, StringComparison.OrdinalIgnoreCase))
			{
				return await ApplyInventoryModifiersAsync();
			}

			if (string.Equals(modifiersMode, DiceGameModifiersMode.Scripted, StringComparison.OrdinalIgnoreCase))
			{
				return await ApplyModifiersScheduleAsync(diceGameConfig.modifiers_set_id);
			}

			FailDiceGameSetup(
				$"[DiceGame] Unsupported modifiers_mode '{diceGameConfig.modifiers_mode}'. Expected '{DiceGameModifiersMode.Inventory}' or '{DiceGameModifiersMode.Scripted}'.");
			return false;
		}

		private UniTask<bool> ApplyInventoryModifiersAsync()
		{
			// Keep player's inventory-driven modifiers intact in default mode.
			ClearRuntimeGlobalModifiers();
			diceGameModel.EnemyModifierItemsModel.Reset();
			diceGameModel.EnemyModifiersModel.Reset();
			return UniTask.FromResult(true);
		}

		private async UniTask<bool> ApplyModifiersScheduleAsync(string setIdOverride)
		{
			var schedule = await configService.GetFirstOrDefaultAsync<DiceGameModifiersScheduleConfig>(ResourcePaths.Json.dice_game_modifiers_schedule);
			if (schedule == null)
			{
				FailDiceGameSetup($"[DiceGame] Modifiers schedule '{ResourcePaths.Json.dice_game_modifiers_schedule}' not found.");
				return false;
			}

			schedule.ParseConfig();
			if (!schedule.TryValidateStatic(out var validationError))
			{
				FailDiceGameSetup($"[DiceGame] Modifiers schedule validation failed: {validationError}");
				return false;
			}

			DiceGameModifierSet resolvedSet = null;
			var overrideSetId = setIdOverride?.Trim();
			if (!string.IsNullOrWhiteSpace(overrideSetId))
			{
				if (!schedule.sets.TryGetValue(overrideSetId, out resolvedSet) || resolvedSet == null)
				{
					FailDiceGameSetup(
						$"[DiceGame] modifiers_set_id override '{overrideSetId}' not found in schedule '{ResourcePaths.Json.dice_game_modifiers_schedule}'.");
					return false;
				}
			}
			else
			{
				var level = run.Level + 1;
				var day = run.Day + 1;
				var match = run.Tick + 1;
				if (!schedule.TryResolveSet(level, day, match, out resolvedSet) || resolvedSet == null)
				{
					FailDiceGameSetup($"[DiceGame] Modifiers schedule could not resolve set for level={level}, day={day}, match={match}.");
					return false;
				}
			}

			ClearRuntimeGlobalModifiers();
			diceGameModel.EnemyModifierItemsModel.Reset();
			diceGameModel.EnemyModifiersModel.Reset();

			return ApplyGlobalModifierSet(resolvedSet.player_modifiers, diceGameModel.PlayerScoringService, "player");
		}

		private bool ApplyGlobalModifierSet(
			string[] modifierIds,
			DiceScoringService scoringService,
			string sideLabel)
		{
			if (modifierIds == null || modifierIds.Length == 0)
			{
				return true;
			}

			var uniqueIds = new HashSet<string>();
			for (int i = 0; i < modifierIds.Length; i++)
			{
				var modifierId = modifierIds[i]?.Trim();
				if (string.IsNullOrWhiteSpace(modifierId))
				{
					FailDiceGameSetup($"[DiceGame] Empty modifier id in {sideLabel}_modifiers.");
					return false;
				}

				if (!uniqueIds.Add(modifierId))
				{
					FailDiceGameSetup($"[DiceGame] Duplicate modifier id '{modifierId}' in {sideLabel}_modifiers.");
					return false;
				}

				var modifier = GlobalModifierFactory.Create(modifierId, scoringService);
				if (modifier == null)
				{
					FailDiceGameSetup($"[DiceGame] Failed to create modifier '{modifierId}' for {sideLabel}.");
					return false;
				}

				diceGameModel.PlayerModifiersModel.AddModifier(modifier);
				runtimeGlobalModifiers.Add(modifier);
			}

			return true;
		}

		private void ClearRuntimeGlobalModifiers()
		{
			if (runtimeGlobalModifiers.Count == 0)
			{
				return;
			}

			var model = diceGameModel.PlayerModifiersModel;
			for (int i = 0; i < runtimeGlobalModifiers.Count; i++)
			{
				model.RemoveModifier(runtimeGlobalModifiers[i]);
			}

			runtimeGlobalModifiers.Clear();
		}

		private void FailDiceGameSetup(string message)
		{
			Debug.LogError(message);
			diceGameModel.SetConditionFailed();
		}
	}
}

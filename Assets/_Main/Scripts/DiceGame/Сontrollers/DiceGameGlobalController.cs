using System;
using System.Collections.Generic;
using System.Threading;
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

	public class DiceGameGlobalController : IBaseController, IActivatable, IGameStateChanger
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
		private readonly IAnalyticsService analyticsService;
		private readonly DiceGameScenarioSetup scenarioSetup;
		private readonly DiceGameItemsDisplayManager itemsDisplayManager;
		private readonly DiceItemViewRegistry itemViewRegistry;

		private readonly SceneContext sceneContext;

		private DiceView[] playerDiceViewsArray;
		private DiceView[] enemyDiceViewsArray;
		private EnemyAiScenarioRuntime enemyScenarioRuntime;
		private Dictionary<string, RunConfig> runConfigs;

		private DicePreGameController dicePreGameController;
		private List<IBaseController> persistentControllers = new();
		private List<IBaseController> gameControllers = new();
		private List<IBaseController> betControllers = new();
		private List<IBaseController> selectionControllers = new();
		private bool gamePreviousStoped = false;
		private bool isMatchResultFlowStarted;
		private bool diceInputsLockedBySpeaking;
		private CancellationTokenSource startDiceGameCts;

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
			analyticsService = serviceLocator.Get<IAnalyticsService>();
			scenarioSetup = new DiceGameScenarioSetup(diceGameModel, run, resourceService);
			itemViewRegistry = new DiceItemViewRegistry();
			itemsDisplayManager = new DiceGameItemsDisplayManager(
				diceGameModel,
				sceneContext.DiceGameTableView,
				lifecycleService,
				objectFactory,
				itemViewRegistry,
				notificationService);
		}

		public void Activate()
		{
			playerModel.PlayerStateModel.StateAdded += OnCharacterStateAddedHandler;
			playerModel.PlayerStateModel.StateRemoved += OnCharacterStateRemovedHandler;
			diceGameModel.OnDiceGameStateChanged += OnDiceGameStateChangedHandler;
			diceGameModel.OnGameConditionPassed += OnGameConditionPassedHandler;
			diceGameModel.OnGameConditionFailed += OnGameConditionFailedHandler;
			diceGameModel.OnDiceAnimationInProgressChanged += OnDiceAnimationInProgressChangedHandler;

			OnDiceGameStateChangedHandler();

			dicePreGameController = new DicePreGameController(sceneContext, playerView, playerModel, run);
			lifecycleService.RegisterAsync(dicePreGameController).Forget();
		}

		public void Deactivate()
		{
			CancelStartDiceGameFlow();
			lifecycleService.Unregister(dicePreGameController);
			dicePreGameController = null;
			playerModel.PlayerStateModel.StateRemoved -= OnPostDialogueFinished;
			isMatchResultFlowStarted = false;
			ReleaseDiceInputsLockedBySpeaking();

			diceGameModel.OnDiceAnimationInProgressChanged -= OnDiceAnimationInProgressChangedHandler;
			playerModel.PlayerStateModel.StateAdded -= OnCharacterStateAddedHandler;
			playerModel.PlayerStateModel.StateRemoved -= OnCharacterStateRemovedHandler;
			diceGameModel.OnDiceGameStateChanged -= OnDiceGameStateChangedHandler;
			diceGameModel.OnGameConditionPassed -= OnGameConditionPassedHandler;
			diceGameModel.OnGameConditionFailed -= OnGameConditionFailedHandler;
		}

		public IEnumerable<(GameStateTransitionTask task, GameStateChangeFunc func)> GetStateChangeFuncs()
		{
			yield return (GameStateTransitionTask.PREPARE_DICE_MODIFIERS, PrepareModifiersForStatsAsync);
		}

		private async UniTask PrepareModifiersForStatsAsync(GameStateTransition _)
		{
			if (playerModel.PlayerStateModel.HasState(CharacterState.DICE_GAME))
			{
				return;
			}

			var diceGameConfig = await configService.GetFirstOrDefaultAsync<DiceGameConfig>(ResourcePaths.Json.dice_game_rules);
			if (diceGameConfig == null)
			{
				FailDiceGameSetup("[DiceGame] dice_game_rules config is missing.");
				return;
			}

			await scenarioSetup.SetupModifiersAsync(diceGameConfig);
		}

		private void OnGameConditionPassedHandler()
		{
			if (isMatchResultFlowStarted)
			{
				return;
			}

			isMatchResultFlowStarted = true;
			HandleMatchResultAsync(true).Forget();
		}

		private void OnGameConditionFailedHandler()
		{
			if (isMatchResultFlowStarted)
			{
				return;
			}

			isMatchResultFlowStarted = true;
			HandleMatchResultAsync(false).Forget();
		}

		private async UniTask HandleMatchResultAsync(bool isWin)
		{
			diceGameModel.BeginDiceAnimation();
			try
			{
				var turnFlowAwaiter = diceGameModel.TurnFlowAwaiter;
				if (turnFlowAwaiter != null)
				{
					await turnFlowAwaiter.WaitForEmptyAsync();
				}

				var scoreAnimationDuration = diceTableView.ScoreAnimationDuration;
				if (scoreAnimationDuration > 0f)
				{
					await UniTask.Delay(TimeSpan.FromSeconds(scoreAnimationDuration));
				}

				if (isWin)
				{
					if (notificationService != null)
					{
						await notificationService.ShowBannerRawAsync("YOU WIN");
					}
				}
				else
				{
					audioService?.PlaySound(SoundNames.Fail);
					if (notificationService != null)
					{
						await notificationService.ShowBannerRawAsync("YOU LOSE", isNegative: true, playSound: false);
					}
				}

				TrackMatchResultAnalytics(isWin);
				if (isWin)
				{
					playerModel.InventoryModel.GiveCash(diceGameModel.CalculateWinPayout());
				}

				playerModel.PlayerStateModel.StateRemoved -= OnPostDialogueFinished;
				playerModel.PlayerStateModel.StateRemoved += OnPostDialogueFinished;
				PostGameDialogue(isWin);
			}
			finally
			{
				diceGameModel.EndDiceAnimation();
			}
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
			else if (playerModel.PlayerStateModel.HasState(CharacterState.DICE_GAME) && state == CharacterState.SPEAKING)
			{
				AcquireDiceInputsLockedBySpeaking();
			}
		}

		// ReSharper disable Unity.PerformanceAnalysis
		private void OnCharacterStateRemovedHandler(CharacterState state)
		{
			if (state == CharacterState.DICE_GAME)
			{
				StopDiceGame();
			}
			else if (state == CharacterState.SPEAKING)
			{
				ReleaseDiceInputsLockedBySpeaking();
			}
		}

		private async UniTask StartDiceGame()
		{
			var cancellationToken = BeginStartDiceGameFlow();
			gamePreviousStoped = false;
			isMatchResultFlowStarted = false;
			inputService.EnableDiceGameInputs();

			inputService.OnDiceGameLeave += OnExitRequested;

			try
			{
				if (!await SetupBaseModels())
				{
					return;
				}

				cancellationToken.ThrowIfCancellationRequested();
				await DiceGamePersistentControllers();
				cancellationToken.ThrowIfCancellationRequested();
				await itemsDisplayManager.SetupItemsDisplayAsync();
				cancellationToken.ThrowIfCancellationRequested();
				await SelectionProcess();
				cancellationToken.ThrowIfCancellationRequested();

				itemsDisplayManager.MoveItemsToGameSlots();

				await BetProcess(cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();
				if (!await SetupEnemyDiceList())
				{
					return;
				}

				cancellationToken.ThrowIfCancellationRequested();
				var turnFlowAwaiter = awaiterService.GetPool("dice.turn_flow");
				diceGameModel.SetTurnFlowAwaiter(turnFlowAwaiter);
				var tableView = sceneContext.DiceGameTableView;
				var upgradeController = new DiceGameUpgradeController(
					diceGameModel,
					run,
					loggerService,
					turnFlowAwaiter,
					objectFactory,
					audioService,
					notificationService,
					localizationService,
					analyticsService,
					tableView);
				var processController = new DiceGameProcessController(
					loggerService, diceGameModel, cameraShakeService, audioService, run, notificationService, turnFlowAwaiter);
				var itemTargetingController = new DiceItemTargetingController(diceGameModel);

				gameControllers.AddRange(new IBaseController[]
				{
					upgradeController,
					processController,
					itemTargetingController,
					new DiceItemTargetingVisualController(diceGameModel, itemViewRegistry, tableView, itemTargetingController),
					new DiceGameUpgradeVisualController(uiService, upgradeController, turnFlowAwaiter, resourceService, loggerService),
					new DiceGameCombinationsDisplayController(diceGameModel, tableView, turnFlowAwaiter, audioService),
					new EnemyTurnController(processController, diceGameModel, enemyScenarioRuntime),
					new DiceGameViewController(tableView, diceGameModel, cameraShakeService, notificationService, localizationService),
					new DiceGameResultController(diceGameModel)
				});

				await lifecycleService.RegisterControllersGroupAsync(gameControllers);
				cancellationToken.ThrowIfCancellationRequested();
				processController.TryStartInitialRoll();
			}
			catch (OperationCanceledException)
			{
			}
		}

		// ReSharper disable Unity.PerformanceAnalysis
		private void StopDiceGame()
		{
			inputService.OnDiceGameLeave -= OnExitRequested;
			playerModel.PlayerStateModel.StateRemoved -= OnPostDialogueFinished;
			isMatchResultFlowStarted = false;
			CancelStartDiceGameFlow();
			ReleaseDiceInputsLockedBySpeaking();

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

			runConfigs ??= await configService.GetConfigsAsync<RunConfig>(ResourcePaths.Json.run_rules);
			if (runConfigs == null || !runConfigs.TryGetValue(run.RunRulesId, out var runConfig) || runConfig == null)
			{
				FailDiceGameSetup(
					$"[DiceGame] run_rules config is missing for selected id '{run.RunRulesId}'.");
				return false;
			}

			if (runConfig.levels == null || run.Level < 0 || run.Level >= runConfig.levels.Length)
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

			if (!TryCalculateStageBaseBet(levelConfig, out var stageBaseBet, out var stageBetError))
			{
				FailDiceGameSetup(stageBetError);
				return false;
			}

			var newTableModel = new TableModel(CouplePositionsHandler.FirstPosArray, CouplePositionsHandler.SecondPosArray);
			diceGameModel.Setup(diceGameConfig, stageBaseBet, playerModel.InventoryModel.CashCount, newTableModel);
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
					new DiceItemAnalyticsController(diceGameModel, analyticsService)
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
				gameControllers.Add(new DiceController(model, view, tableModel, diceGameModel, audioService));
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
				gameControllers.Add(new DiceController(model, view, tableModel, diceGameModel, audioService));
			}

			return true;
		}

		private async UniTask BetProcess(CancellationToken cancellationToken)
		{
			diceGameModel.ChangeDiceGameState(DiceGameState.BET);

			betControllers.AddRange(DiceFactory.GetDiceGameBetControllers(sceneContext, diceGameModel));
			foreach (var controller in betControllers)
			{
				await lifecycleService.RegisterAsync(controller);
			}

			try
			{
				await UniTask.WaitUntil(() => diceGameModel.DiceGameState != DiceGameState.BET, cancellationToken: cancellationToken);

				if (diceGameModel.DiceGameState == DiceGameState.GAME)
				{
					playerModel.InventoryModel.TakeCash(diceGameModel.BetSize);
				}
			}
			finally
			{
				ClenUpBetControllers();
			}
		}

		private CancellationToken BeginStartDiceGameFlow()
		{
			CancelStartDiceGameFlow();
			startDiceGameCts = new CancellationTokenSource();
			return startDiceGameCts.Token;
		}

		private void CancelStartDiceGameFlow()
		{
			if (startDiceGameCts == null)
			{
				return;
			}

			startDiceGameCts.Cancel();
			startDiceGameCts.Dispose();
			startDiceGameCts = null;
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

			ResetRemainingPlayerModifierItems();
		}

		private void ResetRemainingPlayerModifierItems()
		{
			var remainingItems = diceGameModel.PlayerModifierItemsModel?.Items;
			if (remainingItems == null || remainingItems.Count == 0)
			{
				return;
			}

			for (int i = 0; i < remainingItems.Count; i++)
			{
				remainingItems[i]?.ResetItem();
			}

			// Some items clear their bound game model in ResetItem; bind again for the next match.
			diceGameModel.PlayerModifierItemsModel.BindGameModel(diceGameModel);
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

		private static bool TryCalculateStageBaseBet(LevelConfig levelConfig, out int stageBaseBet, out string error)
		{
			stageBaseBet = 0;
			error = null;
			if (levelConfig.cash_goal <= 0)
			{
				error = $"[DiceGame] cash_goal must be > 0 for level '{levelConfig.id}'.";
				return false;
			}

			var matchesLeft = levelConfig.days * levelConfig.ticks_per_day;
			if (matchesLeft <= 0)
			{
				error = $"[DiceGame] matches_left must be > 0 for level '{levelConfig.id}'.";
				return false;
			}

			var halfMatches = Mathf.CeilToInt(matchesLeft * 0.5f);
			var denominator = 3 * halfMatches;
			if (denominator <= 0)
			{
				error = $"[DiceGame] Invalid stage bet denominator for level '{levelConfig.id}'.";
				return false;
			}

			var rawBet = (float)levelConfig.cash_goal / denominator;
			stageBaseBet = CeilToStep(rawBet, 5);
			if (stageBaseBet <= 0)
			{
				error = $"[DiceGame] Computed stage base bet is invalid for level '{levelConfig.id}'.";
				return false;
			}

			return true;
		}

		private static int CeilToStep(float value, int step)
		{
			if (step <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(step), step, "[DiceGame] Ceil step must be > 0.");
			}

			return Mathf.CeilToInt(value / step) * step;
		}

		private void FailDiceGameSetup(string message)
		{
			Debug.LogError(message);
			diceGameModel.SetConditionFailed(
				DiceMatchResultReason.SetupFailed,
				DiceMatchStage.Setup,
				"global_setup");
		}

		private void TrackMatchResultAnalytics(bool isWin)
		{
			if (analyticsService == null)
			{
				return;
			}

			var currentTableModel = tableModel;
			var playerScore = currentTableModel != null ? currentTableModel.PlayerBankedPoints : 0;
			var enemyScore = currentTableModel != null ? currentTableModel.EnemyBankedPoints : 0;
			var targetScore = diceGameModel.TargetPoints;
			var betSize = diceGameModel.BetSize;
			var turnIndex = diceGameModel.CurrentTurn;

			analyticsService.TrackDiceMatchFinished(
				run,
				isWin,
				diceGameModel.MatchResultReason,
				diceGameModel.MatchResultStage,
				playerScore,
				enemyScore,
				targetScore,
				betSize,
				turnIndex,
				diceGameModel.MatchResultSource);
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

		private void OnDiceAnimationInProgressChangedHandler(bool oldValuem, bool newValue)
		{
			if (diceGameModel.IsDiceAnimationInProgress)
			{
				inputService.DisableDiceGameInputs();
			}
			else
			{
				inputService.EnableDiceGameInputs();
			}
		}

		private void AcquireDiceInputsLockedBySpeaking()
		{
			if (diceInputsLockedBySpeaking)
			{
				return;
			}

			diceInputsLockedBySpeaking = true;
			inputService.DisableDiceGameInputs();
		}

		private void ReleaseDiceInputsLockedBySpeaking()
		{
			if (!diceInputsLockedBySpeaking)
			{
				return;
			}

			diceInputsLockedBySpeaking = false;
			inputService.EnableDiceGameInputs();
		}
	}
}

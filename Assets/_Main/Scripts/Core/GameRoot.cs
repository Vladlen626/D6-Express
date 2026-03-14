using System.Collections.Generic;
using _Main.Scripts.Core.Services;
using _Main.Scripts.Dice;
using _Main.Scripts.UI;
using Cysharp.Threading.Tasks;
using FMODUnity;
using PlatformCore.Core;
using PlatformCore.Services;
using PlatformCore.Services.Audio;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;
using UnityEngine;

namespace _Main.Scripts.Core
{
	public class GameRoot : BaseGameRoot
	{
		protected override void RegisterServices(PersistentSceneContext persistentSceneContext)
		{
			Debug.Log("[GameRoot] Register services...");

			var logger = new LoggerService();
			var resourceService = new ResourceService(logger);
			var objectFactory = new ObjectFactory(resourceService, logger);
			var sceneService = new SceneService(logger);
			var inputService = new InputBaseService();
			var audioService = new AudioBaseService(logger);
			var uiService = new UIBaseService(logger, resourceService, persistentSceneContext.UICanvases);
			var cameraService = new CameraService(objectFactory);
			var cursorService = new CursorService(uiService, logger);
			var configService = new ConfigService(resourceService, logger);
			var localizationService = new LocalizationServiceBase(configService);
			var awaiterService = new AsyncAwaiterService();
			var diceScoringService = new DiceScoringService();
			var notificationService = new GlobalNotificationService(uiService, objectFactory, localizationService);
			var analyticsService = new GameAnalyticsService();

			_serviceLocator.Register<ILoggerService, LoggerService>(logger);
			_serviceLocator.Register<IResourceService, ResourceService>(resourceService);
			_serviceLocator.Register<IObjectFactory, ObjectFactory>(objectFactory);
			_serviceLocator.Register<ISceneService, SceneService>(sceneService);
			_serviceLocator.Register<IInputService, InputBaseService>(inputService);
			_serviceLocator.Register<IAudioService, AudioBaseService>(audioService);
			_serviceLocator.Register<IUIService, UIBaseService>(uiService);
			_serviceLocator.Register<ICameraShakeService, CameraService>(cameraService);
			_serviceLocator.Register<ICameraService, CameraService>(cameraService);
			_serviceLocator.Register<ICursorService, CursorService>(cursorService);
			_serviceLocator.Register<ConfigService, ConfigService>(configService);
			_serviceLocator.Register<ILocalizationService, LocalizationServiceBase>(localizationService);
			_serviceLocator.Register<IAsyncAwaiterService, AsyncAwaiterService>(awaiterService);
			_serviceLocator.Register<DiceScoringService, DiceScoringService>(diceScoringService);
			_serviceLocator.Register<GlobalNotificationService, GlobalNotificationService>(notificationService);
			_serviceLocator.Register<IAnalyticsService, GameAnalyticsService>(analyticsService);

			Debug.Log("[GameRoot] Services registered!");
		}

		protected override async UniTask LaunchGameAsync(PersistentSceneContext persistentSceneContext)
		{
			var factory = _serviceLocator.Get<IObjectFactory>();
			var sceneService = _serviceLocator.Get<ISceneService>();
			var audioService = _serviceLocator.Get<IAudioService>();
			var uiService = _serviceLocator.Get<IUIService>();
			var cameraService = _serviceLocator.Get<ICameraService>();
			var cursorService = _serviceLocator.Get<ICursorService>();
			var inputService = _serviceLocator.Get<IInputService>();
			var configService = _serviceLocator.Get<ConfigService>();
			var scoringService = _serviceLocator.Get<DiceScoringService>();
			var notificationService = _serviceLocator.Get<GlobalNotificationService>();
			var analyticsService = _serviceLocator.Get<IAnalyticsService>();
			var localizationService = _serviceLocator.Get<ILocalizationService>();

			var run = new Run();
			var game = new D6Game();

			var playerModel = new PlayerModel();

			var transitionViewController = new TransitionViewController(uiService);
			await _lifecycle.RegisterAsync(transitionViewController);
			await transitionViewController.ShowContext(0);

			await UniTask.WaitUntil(() => RuntimeManager.IsInitialized);
			// Controllers list
			var controllersList = new List<IBaseController>();
			// --------------

			var diceGameModel = new DiceGameModel(playerModel.InventoryModel, scoringService);
			var pauseState = new PauseState();

			// persistent scene load
			var persistentSceneName = sceneService.GetActiveSceneName();
			await UniTask.WaitUntil(() => sceneService.IsSceneLoaded(persistentSceneName));
			// --------------

			await audioService.PlayMusicAsync(SoundNames.StationSound, 0.5f);

			var mainMenuController = new MainMenuController(uiService, game, run);
			await _lifecycle.RegisterAsync(mainMenuController);

			await sceneService.LoadSceneAsync(SceneNames.Train);
			await UniTask.WaitUntil(() => sceneService.IsSceneLoaded(SceneNames.Train));
			sceneService.SetActiveScene(SceneNames.Train);

			if (!sceneService.TryGetSceneContext(SceneNames.Train, out var context))
			{
				Debug.LogError($"[GameRoot] Scene {SceneNames.Train} could not have SceneContext!");
				return;
			}

			var sceneContext = context as SceneContext;

#if UNITY_EDITOR
			var state = DebugVariables.StartSpawnLocation;
#else
				var state = Location.MAIN_MENU;
#endif
			//NPC
			var npcSpawner = NpcFactory.CreateNpcSpawner(factory, game, run, sceneContext.SpawnPoints);

			//Player
			var playerView = await PlayerFactory.SpawnPlayerView(factory, inputService, playerModel,
				state == Location.STATION
					? sceneContext.PlayerStationSpawnPosition
					: sceneContext.PlayerTrainSpawnPosition, sceneContext.InteractionToStateTable);
			playerModel.PlayerStateModel.FillCharacterStatesDict(playerView.CharacterStateHandlers);
			controllersList.AddRange(PlayerFactory.GetPlayerBaseControllers(playerView, _serviceLocator, playerModel,
				inputService, audioService, game, run));

			// Level
			controllersList.AddRange(await RunFactory.GetBaseControllers(game, run, playerModel, playerView,
				configService, cameraService, scoringService));

			controllersList.Add(new AnalyticsController(game, run, analyticsService));

			var winViewController = new WinViewController(uiService, game, inputService, cursorService, configService);
			var loseViewController =
				new LoseViewController(uiService, game, inputService, cursorService, configService);
			var baseControllers = new IBaseController[]
			{
				winViewController,
				loseViewController,
				new SettingsController(uiService, audioService, cursorService, inputService, pauseState),
				new DiceGameUIController(uiService, inputService, playerModel.PlayerStateModel),
				new DiceGameGlobalController(diceGameModel, playerModel, playerView, sceneContext, _serviceLocator,
					run, configService, notificationService),
				new LightController(sceneContext.Lights, run),
				new InformationPanelStationController(run, sceneContext.InformationPanelView, configService),
				new LevelStartModifierController(run, diceGameModel),
				new CameraController(inputService, cameraService, playerModel.PlayerStateModel, game),
				new InventoryController(playerModel.InventoryModel, diceGameModel, factory, configService, audioService,
					sceneContext.InventoryView),
				new ModifierItemsSyncController(playerModel.InventoryModel,
					playerModel.InventoryModel.ModifierItemsModel, configService, scoringService),
				new InventoryModifierItemsController(playerModel.InventoryModel, sceneContext.InventoryView, factory),
				new TooltipsController(uiService, diceGameModel, configService, Camera.main,
					sceneContext.DiceGameTableView),
			};

			var trainShop = await ShopFactory.GetTrainShopAsync(playerModel.InventoryModel, configService);
			var stationShop = await ShopFactory.GetStationShopAsync(playerModel.InventoryModel, configService);

			var sleepController = RunFactory.GetSleepControllers(run, playerView);
			var locationController = new LocationController(game, sceneContext, audioService);
			var playerController = new PlayerController(playerModel, playerView, sceneContext);
			var shopController = new ShopController(run, trainShop, stationShop);
			var statsController = new StatsViewController(uiService, run, configService, inputService, playerModel.InventoryModel.ModifiersModel, factory);

			controllersList.AddRange(new IBaseController[]
			{
				new LedTrainController(run, sceneContext.Leds),
				ShopFactory.GetShopViewController(stationShop, sceneContext.StationShop, factory, playerView.Interactor,
					sceneContext.StationShopkeeper),
				ShopFactory.GetShopViewController(trainShop, sceneContext.TrainShop, factory, playerView.Interactor,
					sceneContext.TrainShopkeeper),
				ShopFactory.GetShopTooltipsController(uiService, stationShop, sceneContext.StationShop, playerView.Interactor, Camera.main),
				ShopFactory.GetShopTooltipsController(uiService, trainShop, sceneContext.TrainShop, playerView.Interactor, Camera.main),
				new ShopPurchaseNotificationController(stationShop, notificationService, configService,
					localizationService, analyticsService, "station"),
				new ShopPurchaseNotificationController(trainShop, notificationService, configService,
					localizationService, analyticsService, "train")
			});

			var debugController = await DebugFactory.GetBaseController(inputService, cursorService, game, run,
				playerModel, playerView, configService, notificationService, diceGameModel);
			var speechController = await SpeechFactory.GetSpeechController(uiService, playerModel, playerView, game,
				run, configService, inputService, diceGameModel);

			controllersList.AddRange(new IBaseController[]
			{
				new CashBalanceNotificationController(playerModel.InventoryModel, notificationService),
				debugController,
				speechController,
				new QuestsViewController(uiService, playerModel.Quests, factory, game),
				new ModifierAppliedNotificationController(playerModel.InventoryModel.ModifierItemsModel,
					notificationService, configService, localizationService),
				new ModifiersViewController(uiService, playerModel.InventoryModel.ModifiersModel, factory,
					configService, pauseState),
				new CombinationsController(playerModel.InventoryModel.ModifiersModel,
					sceneContext.CombinationsView),
				sleepController,
				locationController,
				shopController,
				statsController
			});

			controllersList.AddRange(baseControllers);

			var mainQuestController = new MainQuestContoller(run, playerModel, configService);

			await _lifecycle.RegisterAsync(mainQuestController);

			var questsController = new QuestsController(run, playerModel.Quests, new[]
			{
				mainQuestController
			});

			await _lifecycle.RegisterAsync(questsController);

			await _lifecycle.RegisterControllersGroupAsync(controllersList);

			var gameStateController = new GameStateController(game, run);
			gameStateController.AddTask(async (x) => cursorService.LockCursor(), GameStateTransitionTask.LOCK_CURSOR);
			gameStateController.AddTask(async (x) => cursorService.UnlockCursor(), GameStateTransitionTask.UNLOCK_CURSOR);
			gameStateController.AddTask((x) => npcSpawner.Respawn(), GameStateTransitionTask.NPC_RESPAWN);
			gameStateController.AddTask(async (x) => inputService.DisablePlayerInputs(), GameStateTransitionTask.LOCK_PLAYER_INPUT);
			gameStateController.AddTask(async (x) => inputService.EnablePlayerInputs(), GameStateTransitionTask.UNLOCK_PLAYER_INPUT);
			gameStateController.AddTask(async (x) => playerView.gameObject.SetActive(true), GameStateTransitionTask.ENABLE_CHARACTER);
			gameStateController.AddTask(async (x) =>
			{
				playerView.gameObject.SetActive(false);
			}, GameStateTransitionTask.DISABLE_CHARACTER);
			gameStateController.AddChanger(playerController);
			gameStateController.AddChanger(shopController);
			gameStateController.AddChanger(transitionViewController);
			gameStateController.AddChanger(locationController);
			gameStateController.AddChanger(winViewController);
			gameStateController.AddChanger(loseViewController);
			gameStateController.AddChanger(statsController);

			await _lifecycle.RegisterAsync(gameStateController);

			game.RequestSetLocation(Location.MAIN_MENU);
		}
	}
}

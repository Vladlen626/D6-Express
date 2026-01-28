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
using PlatformCore.Services.Factory.PlatformCore.Services.Factory;
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
			var uiService = new UIBaseService(logger, resourceService, persistentSceneContext.StaticCanvas,
				persistentSceneContext.DynamicCanvas);
			var cameraService = new CameraService(objectFactory);
			var cursorService = new CursorService(uiService);
			var configService = new ConfigService(resourceService, logger);
			var localizationService = new LocalizationServiceBase(configService);

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

			var run = new Run();
			var game = new D6Game();

			var transitionViewController = new TransitionViewController(uiService, run, configService);
			await _lifecycle.RegisterAsync(transitionViewController);
			await transitionViewController.ShowContext(0);

			await UniTask.WaitUntil(() => RuntimeManager.IsInitialized);
			// Controllers list
			var controllersList = new List<IBaseController>();
			// --------------

			var playerModel = await PlayerFactory.CreatePlayerModel(configService);

			var diceGameModel = DiceFactory.CreateDiceGameModel();

			var activeSceneName = sceneService.GetActiveSceneName();
			await UniTask.WaitUntil(() => sceneService.IsSceneLoaded(activeSceneName));

			var mainMenuController = new MainMenuController(uiService, game, run, cursorService);
			await _lifecycle.RegisterAsync(mainMenuController);

			await sceneService.LoadSceneAsync(SceneNames.Train);

			//TODO: Контекст сейчас будет обязательным на игровой сцене
			if (!sceneService.TryGetSceneContext(SceneNames.Train, out var context))
			{
				Debug.LogError($"[GameRoot] Scene {SceneNames.Train} could not have SceneContext!");
				return;
			}

			var sceneContext = context as SceneContext;

#if UNITY_EDITOR
			var state = DebugVariables.StartSpawnLocation;
#else
				var state = LevelState.MAIN_MENU;
#endif

			//NPC
			var npcSpawner = NpcFactory.CreateNpcSpawner(factory, game, run, sceneContext.SpawnPoints);

			//Player
			var playerView = await PlayerFactory.SpawnPlayerView(factory, inputService, playerModel, state == Location.STATION ? sceneContext.PlayerStationSpawnPosition : sceneContext.PlayerTrainSpawnPosition);
			playerModel.PlayerStateModel.FillCharacterStatesDict(playerView.CharacterStateHandlers);
			controllersList.AddRange(PlayerFactory.GetPlayerBaseControllers(playerView, _serviceLocator, playerModel, inputService, audioService, game, run));

			// Level
			controllersList.AddRange(await RunFactory.GetBaseControllers(game, run, playerModel, playerView,
				configService, cameraService));

            var winViewController = new WinViewController(uiService, game, inputService, cursorService, configService);
            var loseViewController = new LoseViewController(uiService, game, inputService, cursorService, configService);

            var baseControllers = new IBaseController[]
			{
				winViewController,
				loseViewController,
				new SettingsController(uiService, audioService, cursorService, inputService),
				new DiceGameGlobalController(diceGameModel, playerModel, sceneContext, _serviceLocator,
					run, configService),
				new LightController(sceneContext.Lights, run),
				new InformationPanelViewController(run, sceneContext.InformationPanelView, configService),
				new LevelStartModifierController(run, diceGameModel),
			};

			var shop = await ShopFactory.GetShopAsync(playerModel.InventoryModel, configService);
			var notifications = NotificationsFactory.CreateNotifications();

			var sleepController = RunFactory.GetSleepControllers(uiService, run, playerView, inputService);
			var locationController = new LocationController(game, run, sceneContext, audioService, playerModel, playerView);

			controllersList.Add(ShopFactory.GetShopViewController(shop, sceneContext.Shop, factory, playerView.Interactor, sceneContext.Shopkeeper));
			controllersList.Add(ShopFactory.GetShopTooltipsController(uiService, shop, playerView.Interactor, Camera.main));
			controllersList.Add(await DebugFactory.GetBaseController(inputService, cursorService, run, playerModel, playerView, configService, notifications));
			controllersList.Add(await SpeechFactory.GetSpeechController(uiService, playerModel, playerView, game, run, configService));
			// todo. не требуется к mvp. раскоментить позже
			// controllersList.Add(QuestFactory.GetController(uiService, playerModel.Quests));
			controllersList.Add(NotificationsFactory.GetNotificationsViewControler(uiService, notifications, factory));
			controllersList.Add(NotificationsFactory.GetNotificationsControler(notifications, playerModel.InventoryModel, configService));
			controllersList.Add(sleepController);
			controllersList.Add(locationController);
			controllersList.AddRange(baseControllers);

			await _lifecycle.RegisterControllersGroupAsync(controllersList);
			
			var gameStateController = new GameStateController(game, run);
			gameStateController.AddTask(async (x) => cursorService.LockCursor(), GameStateTransitionTask.LOCK_CURSOR);
			gameStateController.AddTask(async (x) => cursorService.UnlockCursor(), GameStateTransitionTask.UNLOCK_CURSOR);
			gameStateController.AddTask((x) => npcSpawner.Respawn(), GameStateTransitionTask.NPC_RESPAWN);
			gameStateController.AddTask(async (x) => shop.Restock(), GameStateTransitionTask.SHOP_RESTOCK);
			gameStateController.AddChanger(transitionViewController);
			gameStateController.AddChanger(locationController);
			gameStateController.AddChanger(winViewController);
			gameStateController.AddChanger(loseViewController);
			gameStateController.AddChanger(sleepController);

			await _lifecycle.RegisterAsync(gameStateController);

			game.RequestSetLocation(Location.MAIN_MENU);
		}
	}
}

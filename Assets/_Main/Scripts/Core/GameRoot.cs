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
			var transitionService = new TransitionService();
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
			_serviceLocator.Register<TransitionService, TransitionService>(transitionService);
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
			var transitionService = _serviceLocator.Get<TransitionService>();

			cursorService.UnlockCursor();
			await UniTask.WaitUntil(() => RuntimeManager.IsInitialized);
			// Controllers list
			var controllersList = new List<IBaseController>();
			// --------------

			var playerModel = await PlayerFactory.CreatePlayerModel(configService);
			var runModel = await RunFactory.CreateRunModel(configService);
			var diceGameModel = DiceFactory.CreateDiceGameModel();

			var activeSceneName = sceneService.GetActiveSceneName();
			await UniTask.WaitUntil(() => sceneService.IsSceneLoaded(activeSceneName));

			//Load MainMenu Scene
			var sceneForLoad = SceneNames.MainMenu;
			await sceneService.LoadSceneAsync(sceneForLoad);
			await UniTask.WaitUntil(() => sceneService.IsSceneLoaded(sceneForLoad));

			await audioService.PlayMusicAsync(SoundNames.TrainSound);
			var mainMenuController = new MainMenuController(uiService);
			await _lifecycle.RegisterAsync(mainMenuController);

			await mainMenuController.WaitForStartAsync();

			// Transition View Controller 
			var transitionViewController = new TransitionViewController(uiService, transitionService);
			await _lifecycle.RegisterAsync(transitionViewController);
			await transitionViewController.StartTransition();

			await sceneService.UnloadSceneAsync(sceneForLoad);

			// Start Game
			sceneForLoad = SceneNames.Train;
			await sceneService.LoadSceneAsync(sceneForLoad);
			await UniTask.WaitUntil(() => sceneService.IsSceneLoaded(sceneForLoad));
			sceneService.SetActiveScene(sceneForLoad);

			await audioService.StopMusicAsync(0.2f);
			await audioService.PlayMusicAsync(SoundNames.GameplayEvent, 0.5f);

			//TODO: Контекст сейчас будет обязательным на игровой сцене
			if (!sceneService.TryGetSceneContext(sceneForLoad, out var context))
			{
				Debug.LogError($"[GameRoot] Scene {sceneForLoad} could not have SceneContext!");
				return;
			}

			var sceneContext = context as SceneContext;

#if UNITY_EDITOR
				var state = DebugVariables.StartSpawnLocation;
#else
				var state = LevelState.STATION;
#endif

			//NPC
			var npcSpawner = NpcFactory.CreateNpcSpawner(factory, runModel, sceneContext.SpawnPoints);

			//Player
			var playerView = await PlayerFactory.SpawnPlayerView(factory, inputService, playerModel, state == LevelState.STATION ? sceneContext.PlayerStationSpawnPosition : sceneContext.PlayerTrainSpawnPosition);
			playerModel.PlayerStateModel.FillCharacterStatesDict(playerView.CharacterStateHandlers);
			cameraService.AttachTo(playerView.CameraRoot);
			controllersList.AddRange(PlayerFactory.GetPlayerBaseControllers(playerView, _serviceLocator, playerModel, inputService, audioService));

			// Level
			controllersList.AddRange(RunFactory.GetSleepControllers(runModel.LevelModel, playerView));
			controllersList.AddRange(RunFactory.GetBaseControllers(sceneContext, uiService, runModel,
				playerModel, diceGameModel, playerView, audioService));

			var baseControllers = new IBaseController[]
			{
				new WinViewController(uiService, inputService, cursorService, runModel, configService),
				new LoseViewController(uiService, inputService, cursorService, runModel, configService),
				new SettingsController(uiService, audioService, cursorService, inputService),
				new DiceGameGlobalController(diceGameModel, playerModel, sceneContext, _serviceLocator,
					runModel.LevelModel, configService),
				new DiceTooltipsController(uiService, diceGameModel, configService, Camera.main),
				new LightController(sceneContext.Lights, runModel.LevelModel),
				new LevelStartModifierController(runModel.LevelModel, diceGameModel),
				new InformationPanelViewController(runModel, sceneContext.InformationPanelView),
			};

			var shop = await ShopFactory.GetShopAsync(playerModel.InventoryModel, configService);
			var notifications = NotificationsFactory.CreateNotifications();
			var transitionController = new TransitionController(runModel, playerModel, playerView, sceneContext, audioService, npcSpawner, shop, transitionService);

			controllersList.Add(ShopFactory.GetShopViewController(shop, sceneContext.Shop, factory, playerView.Interactor, sceneContext.Shopkeeper));
			controllersList.Add(ShopFactory.GetShopTooltipsController(uiService, shop, playerView.Interactor, Camera.main));
			controllersList.Add(await DebugFactory.GetBaseController(inputService, cursorService, runModel, playerModel, playerView, configService, notifications));
			controllersList.Add(await SpeechFactory.GetSpeechController(uiService, playerModel, playerView, runModel, configService, runModel.LevelModel));
			// todo. не требуется к mvp. раскоментить позже
			// controllersList.Add(QuestFactory.GetController(uiService, playerModel.Quests));
			controllersList.Add(NotificationsFactory.GetNotificationsViewControler(uiService, notifications, factory));
			controllersList.Add(NotificationsFactory.GetNotificationsControler(notifications, playerModel.InventoryModel, configService));
			controllersList.Add(transitionController);

			controllersList.AddRange(baseControllers);

			foreach (var controller in controllersList)
			{
				await _lifecycle.RegisterAsync(controller);
			}

			await transitionController.StartLocationTransition();
			await transitionViewController.FinishTransition();

			runModel.SetLevelState(state);

			transitionViewController.StartObserving();
			transitionController.StartObserving();
		}
	}
}

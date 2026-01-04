using System.Collections.Generic;
using _Main.Scripts.Core.Services;
using _Main.Scripts.Dice;
using _Main.Scripts.UI;
using Cysharp.Threading.Tasks;
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

			cursorService.UnlockCursor();
			// Controllers list
			var controllersList = new List<IBaseController>();
			// --------------

			var playerModel = PlayerFactory.CreatePlayerModel();
			var levelModel = LevelFactory.CreateLevelModel();
			var diceGameModel = DiceFactory.CreateDiceGameModel();

			var activeSceneName = sceneService.GetActiveSceneName();
			await UniTask.WaitUntil(() => sceneService.IsSceneLoaded(activeSceneName));

			//Load MainMenu Scene
			//var sceneForLoad = SceneNames.MainMenu;
			//await sceneService.LoadSceneAsync(sceneForLoad);
			//await UniTask.WaitUntil(() => sceneService.IsSceneLoaded(sceneForLoad));

			//var mainMenuController = new MainMenuController(uiService);
			//await _lifecycle.RegisterAsync(mainMenuController);

			//await mainMenuController.WaitForStartAsync();
			//await sceneService.UnloadSceneAsync(sceneForLoad);

			// Start Game
			var sceneForLoad = SceneNames.Train;
			await sceneService.LoadSceneAsync(sceneForLoad);
			await UniTask.WaitUntil(() => sceneService.IsSceneLoaded(sceneForLoad));

			//TODO: Сделать статический класс с названиями треков
			await audioService.PlayMusicAsync("event:/GameplayEvent");

			//TODO: Контекст сейчас будет обязательным на игровой сцене
			// Какая-то херня, над править, но пока нет идей как. Дальше посмотрим.
			if (!sceneService.TryGetSceneContext(sceneForLoad, out var context))
			{
				Debug.LogError($"[GameRoot] Scene {sceneForLoad} could not have SceneContext!");
				return;
			}

			var sceneContext = context as SceneContext;

			//Player
			var playerView = await PlayerFactory.SpawnPlayerView(sceneContext, factory, inputService, playerModel);
			playerView.Initialize();
			playerModel.PlayerStateModel.FillCharacterStatesDict(playerView.CharacterStateHandlers);
			cameraService.AttachTo(playerView.CameraRoot);
			controllersList.AddRange(PlayerFactory.GetPlayerBaseControllers(playerView, _serviceLocator, playerModel));

			// Level
			controllersList.AddRange(LevelFactory.GetSleepControllers(levelModel, playerView));
			controllersList.AddRange(LevelFactory.GetBaseControllers(sceneContext, uiService, levelModel,
				playerModel, diceGameModel, playerView));

			var baseControllers = new IBaseController[]
			{
				new LoseScreenController(uiService,inputService, cursorService, levelModel),
				new SettingsController(uiService, audioService, cursorService, inputService),
				new DiceGameGlobalController(diceGameModel, playerModel, sceneContext, _serviceLocator, levelModel),
			};

			controllersList.AddRange(DebugFactory.GetBaseController(inputService, cursorService, levelModel, playerModel, playerView));
			controllersList.Add(SpeechFactory.GetSpeechController(uiService, playerModel, playerView, levelModel));

			controllersList.AddRange(baseControllers);

			foreach (var controller in controllersList)
			{
				await _lifecycle.RegisterAsync(controller);
			}
		}
	}
}
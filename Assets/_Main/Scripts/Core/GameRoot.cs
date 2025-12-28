using System.Collections.Generic;
using _Main.Scripts.Core.Services;
using _Main.Scripts.Dice;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services;
using PlatformCore.Services.Factory;
using PlatformCore.Services.Factory.PlatformCore.Services.Factory;
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

			_serviceLocator.Register<ILoggerService, LoggerService>(logger);
			_serviceLocator.Register<IResourceService, ResourceService>(resourceService);
			_serviceLocator.Register<IObjectFactory, ObjectFactory>(objectFactory);
			_serviceLocator.Register<ISceneService, SceneService>(sceneService);
			_serviceLocator.Register<IInputService, InputBaseService>(inputService);

			Debug.Log("[GameRoot] Services registered!");
		}

		protected override async UniTask LaunchGameAsync(PersistentSceneContext persistentSceneContext)
		{
			var factory = _serviceLocator.Get<IObjectFactory>();
			var logger = _serviceLocator.Get<ILoggerService>();
			var sceneService = _serviceLocator.Get<ISceneService>();
			var controllersList = new List<IBaseController>();
			
			var activeSceneName = sceneService.GetActiveSceneName();
			await UniTask.WaitUntil(() => sceneService.IsSceneLoaded(activeSceneName));

			var sceneForLoad = SceneNames.TrainScene;
			await sceneService.LoadSceneAsync(sceneForLoad);
			await UniTask.WaitUntil(() => sceneService.IsSceneLoaded(sceneForLoad));

			if (!sceneService.TryGetSceneContext(sceneForLoad, out var context))
			{
				logger.LogError($"[GameRoot] SceneContext not found in scene '{sceneForLoad}'!");
			}

			var sceneContext = context as SceneContext;
			if (sceneContext == null)
			{
				logger.LogError($"[GameRoot] SceneContext mistype'{sceneForLoad}'!]");
			}

			if (sceneContext != null)
			{
				controllersList.AddRange(await DiceFactory.GetDiceGameControllers(sceneContext, factory, logger));
			}
			

			foreach (var controller in controllersList)
			{
				await _lifecycle.RegisterAsync(controller);
			}
		}
	}
}
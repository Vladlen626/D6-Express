using System.Collections.Generic;
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

			_serviceLocator.Register<ILoggerService, LoggerService>(logger);
			_serviceLocator.Register<IResourceService, ResourceService>(resourceService);
			_serviceLocator.Register<IObjectFactory, ObjectFactory>(objectFactory);
			_serviceLocator.Register<ISceneService, SceneService>(sceneService);

			Debug.Log("[GameRoot] Services registered!");
		}

		protected override async UniTask LaunchGameAsync(PersistentSceneContext persistentSceneContext)
		{
			var factory = _serviceLocator.Get<IObjectFactory>();
			var logger = _serviceLocator.Get<ILoggerService>();
			var sceneService = _serviceLocator.Get<ISceneService>();
			
			var activeSceneName = sceneService.GetActiveSceneName();
			await UniTask.WaitUntil(() => sceneService.IsSceneLoaded(activeSceneName));

			if (!sceneService.TryGetSceneContext("PersistentScene", out var context))
			{
				logger.LogError($"[GameRoot] SceneContext not found in scene '{activeSceneName}'!");
				return;
			}

			var sceneContext = context as SceneContext;
			if (sceneContext == null)
			{
				logger.LogError($"[GameRoot] SceneContext mistype'{activeSceneName}'!]");
			}

			var diceViews =
				await DiceFactory.SpawnDiceArrayAsync(factory, sceneContext.DicePositionsHandler.DicePositions);
			
			var diceControllers = new List<DiceController>();

			foreach (var diceView in diceViews)
			{
				var model = new DiceModel(new LoadedDiceProfileConfig());
				var controller = new DiceController(model, diceView);
				diceControllers.Add(controller);
			}

			foreach (var diceController in diceControllers)
			{
				await _lifecycle.RegisterAsync(diceController);
			}

		}
	}
}
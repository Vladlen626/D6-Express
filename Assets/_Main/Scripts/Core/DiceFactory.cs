using System.Collections.Generic;
using _Main.Scripts.Dice;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services;
using PlatformCore.Services.Factory;
using UnityEngine;

namespace _Main.Scripts.Core
{
	public static class DiceFactory
	{
		private const string DICE_PREFAB_PATH = "Items/DicePrefab";

		public static async UniTask<IBaseController[]> GetDiceGameControllers(
			SceneContext sceneContext,
			IObjectFactory factory,
			ILoggerService logger)
		{
			var controllersList = new List<IBaseController>();

			if (sceneContext.DiceGameTableView == null)
			{
				return controllersList.ToArray();
			}
			
			var dicePosHandler = sceneContext.DiceGameTableView.DicePositionsHandler;
			
			var diceViews = 
				await SpawnDiceArrayAsync(factory, dicePosHandler.DicePositions);

			var diceModels = new List<DiceModel>();
			var diceGameModel = new DiceGameModel(dicePosHandler.DicePositions, dicePosHandler.BankedPositions);
			var turnModel = new TurnModel();

			foreach (var diceView in diceViews)
			{
				var model = new DiceModel(new LoadedDiceProfileConfig()); 
				var controller = new DiceController(model, diceView, diceGameModel);
				diceModels.Add(model);
				controllersList.Add(controller);
			}
			
			var diceGameControllers = new IBaseController[]
			{
				new DiceGameController(diceGameModel, turnModel, diceModels.ToArray(), sceneContext.DiceGameTableView, logger),
				new DiceGameScoreController(diceGameModel, turnModel, sceneContext.DiceGameTableView)
			};
			
			controllersList.AddRange(diceGameControllers);
			
			return controllersList.ToArray();
		}

		public static async UniTask<DiceView[]> SpawnDiceArrayAsync(
			IObjectFactory factory,
			Transform[] spawnPositions)
		{
			var diceViews = new DiceView[spawnPositions.Length];

			for (int i = 0; i < spawnPositions.Length; i++)
			{

				var diceView = await factory.CreateAsync<DiceView>(
					DICE_PREFAB_PATH,
					spawnPositions[i].position,
					Quaternion.identity
				);

				if (diceView == null)
				{
					Debug.LogError($"[GameRoot] Failed to spawn dice {i}!");
					continue;
				}

				diceViews[i] = diceView;
			}

			return diceViews;
		}
	}
}
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
		public static DiceGameModel CreateDiceGameModel()
		{
			var diceGameModel = new DiceGameModel();

			return diceGameModel;
		}
		
		
		public static IBaseController[] GetDiceGameControllers(
			SceneContext sceneContext,
			ILoggerService logger,
			DiceGameModel diceGameModel,
			TableModel tableModel,
			List<DiceModel> diceModels)
		{
			var controllersList = new List<IBaseController>();

			if (sceneContext.DiceGameTableView == null)
			{
				return controllersList.ToArray();
			}

			var diceGameControllers = new IBaseController[]
			{
				new DiceGameProcessController(tableModel, diceModels.ToArray(), sceneContext.DiceGameTableView, logger),
				new DiceGameScoreViewController(tableModel, sceneContext.DiceGameTableView, diceGameModel),
				new DiceGameResultController(diceGameModel, tableModel)
			};
			
			controllersList.AddRange(diceGameControllers);
			
			return controllersList.ToArray();
		}

		public static IBaseController[] GetDiceGameBetControllers(
			SceneContext sceneContext,
			DiceGameModel diceGameModel)
		{
			var controllersList = new List<IBaseController>();

			if (sceneContext.DiceGameTableView == null)
			{
				return controllersList.ToArray();
			}

			var diceGameControllers = new IBaseController[]
			{
				new DiceGameBetController(diceGameModel, sceneContext.DiceGameTableView),
				new DiceGameBetViewController(diceGameModel, sceneContext.DiceGameTableView),
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
					ResourcePaths.Items.DicePrefab,
					spawnPositions[i].position,
					Quaternion.identity
				);

				if (diceView == null)
				{
					Debug.LogError($"[GameRoot] Failed to spawn dice {i}!");
					continue;
				}
				
				diceView.transform.SetParent(spawnPositions[i]);
				diceView.transform.localRotation = new Quaternion(0,0,0,0);
				diceView.Initialize();

				diceViews[i] = diceView;
			}

			return diceViews;
		}
	}
}
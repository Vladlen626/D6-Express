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
		
		
		public static async UniTask<IBaseController[]> GetDiceGameControllers(
			SceneContext sceneContext,
			IObjectFactory factory,
			ILoggerService logger,
			DiceGameModel diceGameModel)
		{
			var controllersList = new List<IBaseController>();

			if (sceneContext.DiceGameTableView == null)
			{
				return controllersList.ToArray();
			}
			
			var dicePosHandler = sceneContext.DiceGameTableView.DicePositionsHandler;
			
			var diceViews = 
				await SpawnDiceArrayAsync(factory, dicePosHandler.DicePositions);

			var tableModel = new TableModel(dicePosHandler.DicePositions, dicePosHandler.BankedPositions);
			var diceModels = new List<DiceModel>();
			var turnModel = new TurnModel();

			foreach (var diceView in diceViews)
			{
				var model = new DiceModel(new LoadedDiceProfileConfig()); 
				var controller = new DiceController(model, diceView, tableModel);
				diceModels.Add(model);
				controllersList.Add(controller);
			}
			
			var diceGameControllers = new IBaseController[]
			{
				new DiceGameProcessController(tableModel, turnModel, diceModels.ToArray(), sceneContext.DiceGameTableView, logger),
				new DiceGameScoreViewController(tableModel, sceneContext.DiceGameTableView, diceGameModel),
				new DiceGameResultController(diceGameModel, tableModel)
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
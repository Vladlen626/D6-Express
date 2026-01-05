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
				new DiceGameProcessController(tableModel, diceModels.ToArray(), sceneContext.DiceGameTableView, logger, diceGameModel),
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
	}
}
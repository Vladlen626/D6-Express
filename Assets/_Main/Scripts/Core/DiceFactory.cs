using System.Collections.Generic;
using _Main.Scripts.Dice;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services;
using PlatformCore.Services.Audio;
using PlatformCore.Services.Factory;
using UnityEngine;

namespace _Main.Scripts.Core
{
	public static class DiceFactory
	{
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

		public static async UniTask<DiceModel> SpawnDiceViewAsync(
			IObjectFactory factory,
			DiceConfig config,
			Vector3 position,
			Quaternion rotation,
			Transform startPos,
			bool isPlayerDice,
			IAudioService audioService,
			DiceGameModel diceGameModel,
			bool resetYRotation = false,
			bool hideOnSpawn = false)
		{
			var view = await factory.CreateAsync<DiceView>(
				ResourcePaths.Items.DicePrefab,
				position,
				rotation);

			if (!view)
			{
				return null;
			}

			if (startPos)
			{
				view.transform.SetParent(startPos);
			}

			view.Initialize(config.id, isPlayerDice, audioService);

			if (resetYRotation)
			{
				view.ResetYRotation();
			}

			if (hideOnSpawn)
			{
				view.Hide();
			}

			DiceModel model = new DiceModel(config);
			model.SetCurrentPosition(view.transform);
			diceGameModel.AddDiceOnScreen(model, view);

			return model;
		}
	}
}

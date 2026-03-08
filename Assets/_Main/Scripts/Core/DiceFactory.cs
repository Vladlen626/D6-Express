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

			if (!sceneContext.DiceGameTableView)
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
			ItemCatalogEntry config,
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

			var visualId = string.IsNullOrEmpty(config.visualId) ? config.id : config.visualId;
			view.Initialize(visualId, isPlayerDice, audioService);

			if (resetYRotation)
			{
				view.ResetYRotation();
			}

			if (hideOnSpawn)
			{
				view.Hide();
			}

			var weights = ResolveWeights(config);
			DiceModel model = new DiceModel(config.id, weights);
			model.SetCurrentPosition(startPos);
			diceGameModel.AddDiceOnScreen(model, view);

			return model;
		}

		private static int[] ResolveWeights(ItemCatalogEntry config)
		{
			if (config != null && config.TryGetDiceData(out var diceData) && diceData?.weights != null && diceData.weights.Length == 6)
			{
				return diceData.weights;
			}

			return new[] { 1, 1, 1, 1, 1, 1 };
		}
	}
}

using _Main.Scripts.Dice;
using Cysharp.Threading.Tasks;
using PlatformCore.Services;
using PlatformCore.Services.Factory;
using UnityEngine;

namespace _Main.Scripts.Core
{
	public static class DiceFactory
	{
		private const string DICE_PREFAB_PATH = "Items/DicePrefab";

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
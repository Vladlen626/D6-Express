using System.Collections.Generic;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public static class DiceGameSetupUtils
	{
		private const int DefaultBaseCap = 6;

		public static int CalcBaseCap(int activeSlotsCount, int bankSlotsCount)
		{
			return Mathf.Min(DefaultBaseCap, activeSlotsCount, bankSlotsCount);
		}

		public static int CalcMaxBySlots(int bankSlotsCount, int activeSlotsCount)
		{
			return Mathf.Min(bankSlotsCount, activeSlotsCount);
		}

		public static bool TryResolveScriptedEnemyDiceConfigs(
			IReadOnlyDictionary<string, ItemCatalogEntry> catalog,
			string[] scriptedDiceIds,
			int maxBySlots,
			out List<ItemCatalogEntry> configs,
			out string error)
		{
			configs = null;
			error = null;

			if (scriptedDiceIds == null || scriptedDiceIds.Length == 0)
			{
				error = "Scripted dice ids are empty.";
				return false;
			}

			if (scriptedDiceIds.Length > maxBySlots)
			{
				error = $"Scenario requires {scriptedDiceIds.Length} enemy dice, but only {maxBySlots} slots are available.";
				return false;
			}

			var resolved = new List<ItemCatalogEntry>(scriptedDiceIds.Length);
			for (int i = 0; i < scriptedDiceIds.Length; i++)
			{
				var diceId = scriptedDiceIds[i];
				if (!catalog.TryGetValue(diceId, out var diceConfig) || diceConfig.typeEnum != ItemCatalogType.Dice)
				{
					error = $"Scenario dice id '{diceId}' is missing or is not a Dice entry.";
					return false;
				}

				resolved.Add(diceConfig);
			}

			configs = resolved;
			return true;
		}

		public static bool TryResolveDefaultEnemyDiceConfigs(
			IReadOnlyDictionary<string, ItemCatalogEntry> catalog,
			int enemyLimit,
			out List<ItemCatalogEntry> configs,
			out string error)
		{
			configs = null;
			error = null;

			if (!catalog.TryGetValue("default", out var defaultConfig) || defaultConfig.typeEnum != ItemCatalogType.Dice)
			{
				error = "Default dice entry not found in catalog.";
				return false;
			}

			var resolved = new List<ItemCatalogEntry>(Mathf.Max(0, enemyLimit));
			for (int i = 0; i < enemyLimit; i++)
			{
				resolved.Add(defaultConfig);
			}

			configs = resolved;
			return true;
		}
	}
}

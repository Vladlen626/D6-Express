using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public static class ModifierItemFactory
	{
		private static readonly Dictionary<string, DiceItemView> PrefabCache = new();

		public static IModifierItem Create(ItemCatalogEntry entry, DiceScoringService scoringService)
		{
			if (entry == null || entry.typeEnum != ItemCatalogType.Modifier)
			{
				return null;
			}

			var data = entry.data;
			var modifierType = data?["modifierType"]?.ToString();
			if (string.IsNullOrEmpty(modifierType))
			{
				return null;
			}

			var prefab = LoadItemPrefab(entry);

			switch (modifierType)
			{
				case nameof(ExtraDiceCapItem):
				{
					var bonus = ReadInt(data?["bonus"], 4);
					return new ExtraDiceCapItem(entry.id, bonus, prefab);
				}
				case nameof(ModifierSilencerItem):
				{
					var cooldown = ReadInt(data?["cooldownPasses"], 2);
					return new ModifierSilencerItem(entry.id, cooldown, prefab);
				}
				case nameof(PassMultiplierItem):
				{
					var mult = ReadFloat(data?["scoreMultiplier"], 1.5f);
					var perDay = ReadInt(data?["activationsPerDay"], 1);
					return new PassMultiplierItem(entry.id, mult, perDay, prefab);
				}
				case nameof(RerollSelectedItem):
				{
					var cooldown = ReadInt(data?["cooldownPasses"], 2);
					return new RerollSelectedItem(entry.id, scoringService, cooldown, prefab);
				}
				case nameof(StepUpItem):
				{
					var selection = ReadInt(data?["selectionCount"], 3);
					var cooldown = ReadInt(data?["cooldownPasses"], selection);
					return new StepUpItem(entry.id, scoringService, selection, cooldown, prefab);
				}
			}

			return null;
		}

		private static DiceItemView LoadItemPrefab(ItemCatalogEntry entry)
		{
			if (entry == null)
			{
				return null;
			}

			var visualId = string.IsNullOrWhiteSpace(entry.visualId) ? entry.id : entry.visualId;
			if (string.IsNullOrWhiteSpace(visualId))
			{
				return null;
			}

			if (PrefabCache.TryGetValue(visualId, out var cached))
			{
				return cached;
			}

			var prefab = Resources.Load<DiceItemView>($"Items/{visualId}");
			PrefabCache[visualId] = prefab;
			return prefab;
		}

		private static int ReadInt(JToken token, int fallback)
		{
			if (token == null)
			{
				return fallback;
			}

			try
			{
				return token.ToObject<int>();
			}
			catch
			{
				return fallback;
			}
		}

		private static float ReadFloat(JToken token, float fallback)
		{
			if (token == null)
			{
				return fallback;
			}

			try
			{
				return token.ToObject<float>();
			}
			catch
			{
				return fallback;
			}
		}
	}
}

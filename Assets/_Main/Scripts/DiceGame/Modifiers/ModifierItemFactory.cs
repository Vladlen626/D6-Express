using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public static class ModifierItemFactory
	{
		private static readonly Dictionary<string, ItemView> PrefabCache = new();

		public static IModifierItem Create(ItemCatalogEntry entry, DiceScoringService scoringService, InventoryModel inventoryModel = null)
		{
			if (entry == null || entry.typeEnum != ItemCatalogType.ModifierItem)
			{
				return null;
			}

			var data = entry.data;
			var itemType = data?["itemType"]?.ToString();
			if (string.IsNullOrEmpty(itemType))
			{
				return null;
			}

			var prefab = LoadItemPrefab(entry);

			switch (itemType)
			{
				case nameof(ExtraDiceCapItem):
				{
					var bonus = ReadInt(data?["bonus"], 4);
					return new ExtraDiceCapItem(entry.id, bonus, prefab);
				}
				case nameof(InvertAllFacesItem):
				{
					return new InvertAllFacesItem(entry.id, scoringService, prefab);
				}
				case nameof(PairUpItem):
				{
					var selectionCount = ReadInt(data?["selectionCount"], 2);
					return new PairUpItem(entry.id, scoringService, selectionCount, prefab);
				}
				case nameof(MedianBlendItem):
				{
					var selectionCount = ReadInt(data?["selectionCount"], 3);
					return new MedianBlendItem(entry.id, scoringService, selectionCount, prefab);
				}
				case nameof(BankWithoutPassItem):
				{
					return new BankWithoutPassItem(entry.id, scoringService, prefab);
				}
				case nameof(SelectedToOppositeItem):
				{
					return new SelectedToOppositeItem(entry.id, scoringService, prefab);
				}
				case nameof(PassScoreFloorItem):
				{
					return new PassScoreFloorItem(entry.id, scoringService, inventoryModel, prefab);
				}
				case nameof(RerollAllUnsavedItem):
				{
					return new RerollAllUnsavedItem(entry.id, scoringService, prefab);
				}
				case nameof(CopyFaceItem):
				{
					return new CopyFaceItem(entry.id, scoringService, prefab);
				}
				case nameof(TargetDiscountItem):
				{
					var bonus = ReadInt(data?["bonus"], -300);
					return new TargetDiscountItem(entry.id, bonus, prefab);
				}
				case nameof(SelectedDoubleStepItem):
				{
					var stepAmount = ReadInt(data?["stepAmount"], 2);
					return new SelectedDoubleStepItem(entry.id, scoringService, stepAmount, prefab);
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

		private static ItemView LoadItemPrefab(ItemCatalogEntry entry)
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

			var prefab = Resources.Load<ItemView>($"Items/{visualId}");
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

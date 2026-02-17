using System;
using Newtonsoft.Json.Linq;

namespace _Main.Scripts.Dice
{
	public static class ModifierItemFactory
	{
		public static IModifierItem Create(ItemCatalogEntry entry)
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

			switch (modifierType)
			{
				case nameof(MultiplyComboModifier):
				{
					var combo = ParseCombination(data?["combination"]) ?? DiceCombination.ThreeOfAKind;
					return new MultiplyComboModifier(entry.id, combo);
				}
				case nameof(MultiplyKindOfModifiers):
				{
					var combo = ParseCombination(data?["combination"]) ?? DiceCombination.SingleOnes;
					var face = ReadInt(data?["face"], 1);
					return new MultiplyKindOfModifiers(entry.id, combo, face);
				}
				case nameof(ShakeRerollModifier):
				{
					var chance = ReadFloat(data?["shakeChance"], 0.95f);
					var duration = ReadFloat(data?["rerollAnimationDuration"], 0.5f);
					return new ShakeRerollModifier(entry.id, chance, duration);
				}
				case nameof(ScrambleCombinationsModifier):
				{
					return new ScrambleCombinationsModifier(entry.id);
				}
				case nameof(PassActivationMultiplierModifier):
				{
					return new PassActivationMultiplierModifier(entry.id);
				}
				case nameof(AdjustTicksPerDayModifier):
				{
					var delta = ReadInt(data?["delta"], 1);
					return new AdjustTicksPerDayModifier(entry.id, delta);
				}
				case nameof(ExtraDiceCapItem):
				{
					var bonus = ReadInt(data?["bonus"], 4);
					return new ExtraDiceCapItem(entry.id, bonus);
				}
				case nameof(ModifierSilencerItem):
				{
					var cooldown = ReadInt(data?["cooldownPasses"], 2);
					return new ModifierSilencerItem(entry.id, cooldown);
				}
				case nameof(PassMultiplierItem):
				{
					var mult = ReadFloat(data?["scoreMultiplier"], 1.5f);
					var perDay = ReadInt(data?["activationsPerDay"], 1);
					return new PassMultiplierItem(entry.id, mult, perDay);
				}
				case nameof(RerollSelectedItem):
				{
					var cooldown = ReadInt(data?["cooldownPasses"], 2);
					return new RerollSelectedItem(entry.id, cooldown);
				}
				case nameof(StepUpItem):
				{
					var selection = ReadInt(data?["selectionCount"], 3);
					var cooldown = ReadInt(data?["cooldownPasses"], selection);
					return new StepUpItem(entry.id, selection, cooldown);
				}
			}

			return null;
		}

		private static DiceCombination? ParseCombination(JToken token)
		{
			var value = token?.ToString();
			if (string.IsNullOrEmpty(value))
			{
				return null;
			}

			if (Enum.TryParse(value, out DiceCombination combo))
			{
				return combo;
			}

			return null;
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

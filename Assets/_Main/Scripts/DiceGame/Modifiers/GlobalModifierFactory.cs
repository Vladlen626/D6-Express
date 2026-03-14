using System;
using System.Collections.Generic;

namespace _Main.Scripts.Dice
{
	public static class GlobalModifierFactory
	{
		private static readonly Dictionary<string, Func<DiceScoringService, IModifier>> Builders =
			new(StringComparer.Ordinal)
			{
				{
					"multiply_combo_three_kind",
					_ => new MultiplyComboModifier(
						DiceCombination.ThreeOfAKind,
						deltaMultiplier: 1,
						uiConfigId: "multiply_combo_three_kind")
				},
				{
					"multiply_kind_single_ones",
					_ => new MultiplyKindOfModifiers(
						DiceCombination.SingleOnes,
						face: 1,
						deltaMultiplier: 1,
						uiConfigId: "multiply_kind_single_ones")
				},
				{
					"three_kind_plus100",
					_ => new FlatComboBonusModifier(
						DiceCombination.ThreeOfAKind,
						bonusScore: 100,
						uiConfigId: "three_kind_plus100")
				},
				{
					"straight_x2",
					_ => new MultiplyComboModifier(
						DiceCombination.StraightLength5,
						deltaMultiplier: 1,
						uiConfigId: "straight_x2",
						matchStraightFamily: true)
				},
				{
					"ones_plus50",
					_ => new FaceScoreBonusModifier(
						faceValue: 1,
						bonusPerScoringDie: 50,
						uiConfigId: "ones_plus50")
				},
				{
					"shake_reroll",
					scoringService => CreateRerollLowestUnsaved(scoringService, "shake_reroll")
				},
				{
					"reroll_lowest_unsaved",
					scoringService => CreateRerollLowestUnsaved(scoringService, "reroll_lowest_unsaved")
				},
				{
					"lowest_unsaved_plus1",
					scoringService => CreateAdjustLowestUnsaved(scoringService, 1, "lowest_unsaved_plus1")
				},
				{
					"scramble_combinations",
					scoringService => scoringService == null
						? null
						: new ScrambleCombinationsModifier(scoringService, uiConfigId: "scramble_combinations")
				},
				{
					"pass_activation_multiplier",
					_ => null
				},
				{
					"adjust_ticks_plus1",
					_ => new AdjustTicksPerDayModifier(delta: 1, uiConfigId: "adjust_ticks_plus1")
				},
				{
					"ticks_plus1",
					_ => new AdjustRunScalarModifier(
						RunScalarTarget.TicksPerDay,
						delta: 1,
						revertOnLevelOrRunEnd: true,
						uiConfigId: "ticks_plus1")
				}
			};

		public static IModifier Create(string modifierId, DiceScoringService scoringService)
		{
			if (string.IsNullOrWhiteSpace(modifierId))
			{
				return null;
			}

			if (!Builders.TryGetValue(modifierId, out var builder))
			{
				return null;
			}

			return builder(scoringService);
		}

		private static IModifier CreateRerollLowestUnsaved(DiceScoringService scoringService, string uiConfigId)
		{
			if (scoringService == null)
			{
				return null;
			}

			return new RerollUnsavedDieModifier(
				scoringService,
				UnsavedDieSelectionStrategy.Lowest,
				chance: 1f,
				rerollAnimationDuration: 0.35f,
				uiConfigId: uiConfigId);
		}

		private static IModifier CreateAdjustLowestUnsaved(DiceScoringService scoringService, int delta, string uiConfigId)
		{
			if (scoringService == null)
			{
				return null;
			}

			return new AdjustUnsavedDieValueModifier(
				scoringService,
				UnsavedDieSelectionStrategy.Lowest,
				delta,
				minValue: 1,
				maxValue: 6,
				uiConfigId: uiConfigId);
		}
	}
}

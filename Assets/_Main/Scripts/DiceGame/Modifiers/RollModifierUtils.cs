using System.Collections.Generic;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	internal static class RollModifierUtils
	{
		public static DiceModel SelectUnsavedDie(
			DiceModifierContext modifierContext,
			UnsavedDieSelectionStrategy selectionStrategy)
		{
			var candidates = GetUnsavedActiveDice(modifierContext);
			if (candidates.Count == 0)
			{
				return null;
			}

			switch (selectionStrategy)
			{
				case UnsavedDieSelectionStrategy.Leftmost:
					return candidates[0];

				case UnsavedDieSelectionStrategy.Lowest:
					return SelectByValue(candidates, preferHighest: false);

				case UnsavedDieSelectionStrategy.Highest:
					return SelectByValue(candidates, preferHighest: true);

				case UnsavedDieSelectionStrategy.Random:
					return candidates[Random.Range(0, candidates.Count)];
			}

			return null;
		}

		public static void RefreshCombinationResult(DiceModifierContext modifierContext, DiceScoringService scoringService)
		{
			if (scoringService == null || modifierContext.Dice == null)
			{
				return;
			}

			var targetList = modifierContext.CombinationResult.Combinations;
			if (targetList == null)
			{
				return;
			}

			var updatedResult = scoringService.Evaluate(DiceGameUtils.GetDiceValues(modifierContext.Dice));
			targetList.Clear();
			targetList.AddRange(updatedResult.Combinations);
		}

		private static List<DiceModel> GetUnsavedActiveDice(DiceModifierContext modifierContext)
		{
			var result = new List<DiceModel>();
			if (modifierContext == null || modifierContext.Dice == null || modifierContext.Table == null)
			{
				return result;
			}

			for (int i = 0; i < modifierContext.Dice.Length; i++)
			{
				var dice = modifierContext.Dice[i];
				if (dice == null || dice.IsSaved)
				{
					continue;
				}

				if (!modifierContext.Table.IsActivePosition(dice.CurrentPosition))
				{
					continue;
				}

				result.Add(dice);
			}

			return result;
		}

		private static DiceModel SelectByValue(IReadOnlyList<DiceModel> candidates, bool preferHighest)
		{
			var selected = candidates[0];
			for (int i = 1; i < candidates.Count; i++)
			{
				var candidate = candidates[i];
				if (preferHighest)
				{
					if (candidate.CurrentValue > selected.CurrentValue)
					{
						selected = candidate;
					}
				}
				else
				{
					if (candidate.CurrentValue < selected.CurrentValue)
					{
						selected = candidate;
					}
				}
			}

			return selected;
		}
	}
}

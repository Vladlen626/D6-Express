using System.Collections.Generic;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public static class DiceGameUtils
	{
		public static DiceCombinationResult GetCombinations(int[] values)
		{
			return DiceScoringService.Instance.Evaluate(values);
		}

		public static int CalculateTotalScore(DiceCombinationResult result)
		{
			return DiceScoringService.Instance.CalculateTotalScore(result);
		}

		public static bool HasTrashInSelected(int[] values)
		{
			return DiceScoringService.Instance.HasTrash(values);
		}

		public static string GetCombinationName(DiceCombination combination)
		{
			return DiceScoringService.Instance.GetDisplayName(null, combination);
		}

		public static string GetCombinationName(string combinationId, DiceCombination combination = DiceCombination.None)
		{
			return DiceScoringService.Instance.GetDisplayName(combinationId, combination);
		}

		public static void UpdateBaseScore(string combinationId, int newBaseScore)
		{
			DiceScoringService.Instance.UpdateBaseScore(combinationId, newBaseScore);
		}

		public static void AddOrReplaceRule(ComboRuleDefinition definition)
		{
			DiceScoringService.Instance.AddOrReplaceRule(definition);
		}

		public static void RemoveRule(string ruleId)
		{
			DiceScoringService.Instance.RemoveRule(ruleId);
		}

		public static void ReorderRules(List<string> orderedIds)
		{
			DiceScoringService.Instance.ReorderRules(orderedIds);
		}

		public static void ReloadScoringDefaults()
		{
			DiceScoringService.Instance.ReloadDefaults();
		}

		public static int GetWeightedRandomValue(int[] weights)
		{
			float totalWeight = 0f;
			foreach (float weight in weights)
			{
				totalWeight += weight;
			}

			float randomValue = Random.Range(0f, totalWeight);
			float cumulativeWeight = 0f;

			for (int i = 0; i < weights.Length; i++)
			{
				cumulativeWeight += weights[i];
				if (randomValue <= cumulativeWeight)
				{
					return i + 1;
				}
			}

			return 1;
		}

		public static int[] GetDiceValues(DiceModel[] dice)
		{
			int[] values = new int[dice.Length];
			for (int i = 0; i < dice.Length; i++)
			{
				values[i] = dice[i].CurrentValue;
			}

			return values;
		}
	}
}

using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class ScrambleCombinationsModifier : IOnPassModifier
	{
		public UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			Debug.Log("FUCK");
			var combinations = modifierContext.CombinationResult.Combinations;
			if (combinations == null || combinations.Count < 2)
			{
				Debug.Log(combinations);
				Debug.Log("NO COMBOS");
				return UniTask.CompletedTask;
			}

			var scrambledEntries = new List<(DiceCombination Combination, int BaseScore)>(combinations.Count);
			foreach (var entry in combinations)
			{
				scrambledEntries.Add((entry.Combination, entry.BaseScore));
			}

			for (int i = scrambledEntries.Count - 1; i > 0; i--)
			{
				int swapIndex = Random.Range(0, i + 1);
				(scrambledEntries[i], scrambledEntries[swapIndex]) = (scrambledEntries[swapIndex], scrambledEntries[i]);
			}

			for (int i = 0; i < combinations.Count; i++)
			{
				combinations[i].Combination = scrambledEntries[i].Combination;
				combinations[i].BaseScore = scrambledEntries[i].BaseScore;
			}

			var logBuilder = new StringBuilder();
			logBuilder.AppendLine("[ScrambleCombinationsModifier] New scrambled combinations:");
			for (int i = 0; i < combinations.Count; i++)
			{
				var entry = combinations[i];
				logBuilder.AppendLine(
					$" #{i + 1}: {DiceGameUtils.GetCombinationName(entry.Combination)} | face {entry.Face} x{entry.Count} | base {entry.BaseScore} | mult {entry.Multiplier} | final {entry.FinalScore}");
			}
			Debug.Log(logBuilder.ToString());

			return UniTask.CompletedTask;
		}
	}
}

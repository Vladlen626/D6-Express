using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random=UnityEngine.Random;

namespace _Main.Scripts.Dice
{
	public class ScrambleCombinationsModifier : IOnPassModifier, IOnRollModifier
	{
		private static readonly DiceCombination[] AvailableCombinations = Enum
			.GetValues(typeof(DiceCombination))
			.Cast<DiceCombination>()
			.Where(c => c != DiceCombination.None)
			.ToArray();

		private static readonly Dictionary<DiceCombination, int[]> CombinationSamples = new()
		{
			{ DiceCombination.Straight_1_6, new[] { 1, 2, 3, 4, 5, 6 } },
			{ DiceCombination.Straight_1_5, new[] { 1, 2, 3, 4, 5 } },
			{ DiceCombination.Straight_2_6, new[] { 2, 3, 4, 5, 6 } },
			{ DiceCombination.ThreeOfAKind, new[] { 1, 1, 1 } },
			{ DiceCombination.FourOfAKind, new[] { 1, 1, 1, 1 } },
			{ DiceCombination.FiveOfAKind, new[] { 1, 1, 1, 1, 1 } },
			{ DiceCombination.SixOfAKind, new[] { 1, 1, 1, 1, 1, 1 } },
			{ DiceCombination.SingleOnes, new[] { 1 } },
			{ DiceCombination.SingleFives, new[] { 5 } }
		};

		private readonly Dictionary<DiceCombination, int> scrambledScores = new ();
		private TableModel currentTable;
		private int lastKnownTurn = -1;

		public UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			var combinations = modifierContext.CombinationResult.Combinations;
			if (combinations == null || combinations.Count == 0)
			{
				return UniTask.CompletedTask;
			}

			if (NeedNewRoundMap(modifierContext))
			{
				Debug.Log("[ScrambleCombinationsModifier] Building new scramble map (new round/game detected).");
				BuildNewRoundMap();
				currentTable = modifierContext.Table;
				lastKnownTurn = modifierContext.DiceGameModel?.CurrentTurn ?? -1;
				LogScrambledScores();
			}

			// Only mutate scoring when points are about to be banked.
			if (modifierContext.Stage == ModifierStage.Pass && scrambledScores.Count > 0)
			{
				Debug.Log("[ScrambleCombinationsModifier] Applying scramble on Pass.");
				ApplyScramble(combinations);
				LogScramble(combinations);
			}
			else
			{
				Debug.Log($"[ScrambleCombinationsModifier] Skipped scramble. Stage={modifierContext.Stage}, mapCount={scrambledScores.Count}.");
			}

			return UniTask.CompletedTask;
		}

		private bool NeedNewRoundMap(DiceModifierContext context)
		{
			if (scrambledScores.Count == 0)
			{
				return true;
			}

			if (context.Table != null && context.Table != currentTable)
			{
				return true;
			}

			int currentTurn = context.DiceGameModel?.CurrentTurn ?? -1;
			if (currentTurn >= 0 && (lastKnownTurn == -1 || currentTurn < lastKnownTurn))
			{
				return true;
			}

			return false;
		}

		private void BuildNewRoundMap()
		{
			var sourceCombinations = new List<DiceCombination>(AvailableCombinations);
			Shuffle(sourceCombinations);
			EnsureDerangement(sourceCombinations);

			scrambledScores.Clear();
			for (int i = 0; i < AvailableCombinations.Length; i++)
			{
				var target = AvailableCombinations[i];
				var source = sourceCombinations[i];
				scrambledScores[target] = GetBaseScoreFromSample(source);
			}
		}

		private void LogScrambledScores()
		{
			var sb = new StringBuilder();
			sb.AppendLine("[ScrambleCombinationsModifier] Scrambled score map:");
			foreach (var pair in scrambledScores)
			{
				sb.AppendLine($" - {DiceGameUtils.GetCombinationName(pair.Key)} -> {pair.Value}");
			}
			Debug.Log(sb.ToString());
		}

		private static int GetBaseScoreFromSample(DiceCombination combination)
		{
			if (!CombinationSamples.TryGetValue(combination, out var sample))
			{
				return 0;
			}

			var result = DiceGameUtils.GetCombinations(sample);
			foreach (var entry in result.Combinations)
			{
				if (entry.Combination == combination)
				{
					return entry.BaseScore;
				}
			}

			return 0;
		}

		private static void EnsureDerangement(IList<DiceCombination> permutation)
		{
			for (int i = 0; i < permutation.Count; i++)
			{
				if (permutation[i] == AvailableCombinations[i])
				{
					int swapIndex = (i + 1) % permutation.Count;
					(permutation[i], permutation[swapIndex]) = (permutation[swapIndex], permutation[i]);
				}
			}
		}

		private static void Shuffle<T>(IList<T> list)
		{
			for (int i = list.Count - 1; i > 0; i--)
			{
				int swapIndex = Random.Range(0, i + 1);
				(list[i], list[swapIndex]) = (list[swapIndex], list[i]);
			}
		}

		private void ApplyScramble(List<DiceCombinationEntry> combinations)
		{
			for (int i = 0; i < combinations.Count; i++)
			{
				var entry = combinations[i];
				if (scrambledScores.TryGetValue(entry.Combination, out var scrambledScore))
				{
					entry.BaseScore = scrambledScore;
				}
			}
		}

		private static void LogScramble(List<DiceCombinationEntry> combinations)
		{
			var logBuilder = new StringBuilder();
			logBuilder.AppendLine("[ScrambleCombinationsModifier] New scrambled combinations:");
			for (int i = 0; i < combinations.Count; i++)
			{
				var entry = combinations[i];
				logBuilder.AppendLine(
					$" #{i + 1}: {DiceGameUtils.GetCombinationName(entry.Combination)} | face {entry.Face} x{entry.Count} | base {entry.BaseScore} | mult {entry.Multiplier} | final {entry.FinalScore}");
			}
			Debug.Log(logBuilder.ToString());
		}
	}
}

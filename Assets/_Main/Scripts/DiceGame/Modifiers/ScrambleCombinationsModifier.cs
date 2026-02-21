using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random=UnityEngine.Random;

namespace _Main.Scripts.Dice
{
	public class ScrambleCombinationsModifier : ModifierItemBase, IOnRoundStartModifier, IOnRollModifier
	{
		private readonly DiceScoringService scoringService;
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
			{ DiceCombination.StraightLength4, new[] { 1, 2, 3, 4 } },
			{ DiceCombination.StraightLength5, new[] { 1, 2, 3, 4, 5 } },
			{ DiceCombination.StraightLength6, new[] { 1, 2, 3, 4, 5, 6 } },
			{ DiceCombination.ThreeOfAKind, new[] { 1, 1, 1 } },
			{ DiceCombination.FourOfAKind, new[] { 1, 1, 1, 1 } },
			{ DiceCombination.FiveOfAKind, new[] { 1, 1, 1, 1, 1 } },
			{ DiceCombination.SixOfAKind, new[] { 1, 1, 1, 1, 1, 1 } },
			{ DiceCombination.SingleOnes, new[] { 1 } },
			{ DiceCombination.SingleFives, new[] { 5 } }
		};

		private readonly Dictionary<DiceCombination, int> scrambledScores = new ();

		public ScrambleCombinationsModifier(string id, DiceScoringService scoringService)
			: base(id, id, DiceItemActivationType.Passive)
		{
			this.scoringService = scoringService;
		}

		public override UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			switch (modifierContext.Stage)
			{
				case ModifierStage.RoundStart:
					Debug.Log("[ScrambleCombinationsModifier] Building new scramble map for round start.");
					BuildNewRoundMap();
					LogScrambledScores();
					ScrambleCombinationsOverlay.UpdateMap(scoringService, scrambledScores);
					break;

				case ModifierStage.Roll:
					ApplyScramble(modifierContext.CombinationResult.Combinations);
					break;

				default:
					Debug.Log($"[ScrambleCombinationsModifier] Skipped scramble. Stage={modifierContext.Stage}, mapCount={scrambledScores.Count}.");
					break;
			}

			return UniTask.CompletedTask;
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
				sb.AppendLine($" - {scoringService.GetDisplayName(null, pair.Key)} -> {pair.Value}");
			}
			Debug.Log(sb.ToString());
		}

		private static int GetBaseScoreFromSample(DiceCombination combination)
		{
			if (!CombinationSamples.TryGetValue(combination, out var sample))
			{
				return 0;
			}

			var result = scoringService.Evaluate(sample);
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
			if (combinations == null || combinations.Count == 0)
			{
				return;
			}

			if (scrambledScores.Count == 0)
			{
				Debug.Log("[ScrambleCombinationsModifier] Scramble map missing on Roll. Rebuilding.");
				BuildNewRoundMap();
				LogScrambledScores();
				ScrambleCombinationsOverlay.UpdateMap(scoringService, scrambledScores);
			}

			Debug.Log("[ScrambleCombinationsModifier] Applying scramble on Roll.");
			for (int i = 0; i < combinations.Count; i++)
			{
				var entry = combinations[i];
				if (scrambledScores.TryGetValue(entry.Combination, out var scrambledScore))
				{
					entry.BaseScore = scrambledScore;
				}
			}

			LogScramble(combinations);
		}

		private static void LogScramble(List<DiceCombinationEntry> combinations)
		{
			var logBuilder = new StringBuilder();
			logBuilder.AppendLine("[ScrambleCombinationsModifier] New scrambled combinations:");
			for (int i = 0; i < combinations.Count; i++)
			{
				var entry = combinations[i];
				logBuilder.AppendLine(
					$" #{i + 1}: {scoringService.GetDisplayName(null, entry.Combination)} | face {entry.Face} x{entry.Count} | base {entry.BaseScore} | mult {entry.Multiplier} | final {entry.FinalScore}");
			}
			Debug.Log(logBuilder.ToString());
		}
	}
}

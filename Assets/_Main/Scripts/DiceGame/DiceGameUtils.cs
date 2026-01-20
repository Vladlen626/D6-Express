using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public static class DiceGameUtils
	{
		private class DiceInfo
		{
			public int[] Counts = new int[7];
			public int[] Remaining = new int[7];
		}


		private static DiceInfo AnalyzeValues(int[] values)
		{
			var info = new DiceInfo();

			if (values == null || values.Length == 0)
			{
				return info;
			}

			foreach (var value in values)
			{
				if (value >= 1 && value <= 6)
				{
					info.Counts[value]++;
					info.Remaining[value]++;
				}
			}

			return info;
		}
		
		private static void TryAddStraight(
			DiceInfo info,
			int[] faces,
			DiceCombination combination,
			List<DiceCombinationEntry> result)
		{
			while (true)
			{
				foreach (int face in faces)
				{
					if (info.Remaining[face] <= 0)
					{
						return;
					}
				}

				foreach (int face in faces)
				{
					info.Remaining[face]--;
				}

				result.Add(new DiceCombinationEntry
				{
					Combination = combination,
					Face = 0,
					Count = faces.Length
				});
			}
		}
		
		public static int CalculateTotalScore(DiceCombinationResult result)
		{
			int total = 0;
			foreach (var combo in result.Combinations)
			{
				total += combo.FinalScore;
			}
			return total;
		}


		private static int ScoreNOfKind(int face, int count)
		{
			if (count < 3)
				return 0;

			int baseScore = (face == 1) ? 1000 : face * 100;

			for (int i = 3; i < count; i++)
				baseScore *= 2;

			return baseScore;
		}

		// === ГЛАВНЫЙ МЕТОД ===
		public static DiceCombinationResult GetCombinations(int[] values)
		{
			var result = new DiceCombinationResult
			{
				Combinations = new List<DiceCombinationEntry>()
			};

			if (values == null || values.Length == 0)
			{
				return result;
			}

			var info = AnalyzeValues(values);

			// === СТРЕЙТЫ ===
			TryAddStraight(info, new[] { 1, 2, 3, 4, 5, 6 }, DiceCombination.Straight_1_6, result.Combinations);
			TryAddStraight(info, new[] { 1, 2, 3, 4, 5 }, DiceCombination.Straight_1_5, result.Combinations);
			TryAddStraight(info, new[] { 2, 3, 4, 5, 6 }, DiceCombination.Straight_2_6, result.Combinations);

			// === N OF A KIND ===
			for (int face = 1; face <= 6; face++)
			{
				while (info.Remaining[face] >= 3)
				{
					int count = info.Remaining[face];
					if (count > 6)
					{
						count = 6;
					}

					DiceCombination combo;
					switch (count)
					{
						case 3: combo = DiceCombination.ThreeOfAKind; break;
						case 4: combo = DiceCombination.FourOfAKind; break;
						case 5: combo = DiceCombination.FiveOfAKind; break;
						default: combo = DiceCombination.SixOfAKind; break;
					}

					info.Remaining[face] -= count;

					result.Combinations.Add(new DiceCombinationEntry
					{
						Combination = combo,
						Face = face,
						Count = count
					});
				}
			}

			// === ОДИНОЧКИ ===
			if (info.Remaining[1] > 0)
			{
				result.Combinations.Add(new DiceCombinationEntry
				{
					Combination = DiceCombination.SingleOnes,
					Face = 1,
					Count = info.Remaining[1]
				});

				info.Remaining[1] = 0;
			}

			if (info.Remaining[5] > 0)
			{
				result.Combinations.Add(new DiceCombinationEntry
				{
					Combination = DiceCombination.SingleFives,
					Face = 5,
					Count = info.Remaining[5]
				});

				info.Remaining[5] = 0;
			}

			BaseScoreSetup(result);

			return result;
		}

		public static bool HasTrashInSelected(int[] values)
		{
			if (values == null || values.Length == 0)
			{
				return false;
			}

			var info = AnalyzeValues(values);

			var combos = GetCombinations(values);
			foreach (var combo in combos.Combinations)
			{
				switch (combo.Combination)
				{
					case DiceCombination.Straight_1_6:
						Consume(info, new[] { 1, 2, 3, 4, 5, 6 });
						break;

					case DiceCombination.Straight_1_5:
						Consume(info, new[] { 1, 2, 3, 4, 5 });
						break;

					case DiceCombination.Straight_2_6:
						Consume(info, new[] { 2, 3, 4, 5, 6 });
						break;

					case DiceCombination.ThreeOfAKind:
					case DiceCombination.FourOfAKind:
					case DiceCombination.FiveOfAKind:
					case DiceCombination.SixOfAKind:
						info.Remaining[combo.Face] -= combo.Count;
						break;

					case DiceCombination.SingleOnes:
						info.Remaining[1] -= combo.Count;
						break;

					case DiceCombination.SingleFives:
						info.Remaining[5] -= combo.Count;
						break;
				}
			}

			// если что-то осталось — это мусор
			for (int face = 1; face <= 6; face++)
			{
				if (info.Remaining[face] > 0)
				{
					return true;
				}
			}

			return false;
		}

		private static void Consume(DiceInfo info, int[] faces)
		{
			foreach (var face in faces)
			{
				info.Remaining[face]--;
			}
		}



		// === УДОБНЫЕ ОБЁРТКИ ДЛЯ ТЕКУЩЕГО КОДА ===

		private static void BaseScoreSetup(DiceCombinationResult combinations)
		{
			foreach (var entry in combinations.Combinations)
			{
				switch (entry.Combination)
				{
					case DiceCombination.Straight_1_6:
						entry.BaseScore = 1500;
						break;

					case DiceCombination.Straight_1_5:
						entry.BaseScore = 500;
						break;

					case DiceCombination.Straight_2_6:
						entry.BaseScore = 750;
						break;

					case DiceCombination.ThreeOfAKind:
					case DiceCombination.FourOfAKind:
					case DiceCombination.FiveOfAKind:
					case DiceCombination.SixOfAKind:
						entry.BaseScore = ScoreNOfKind(entry.Face, entry.Count);
						break;

					case DiceCombination.SingleOnes:
						entry.BaseScore = entry.Count * 100;
						break;

					case DiceCombination.SingleFives:
						entry.BaseScore = entry.Count * 50;
						break;
				}
			}
		}

		public static string GetCombinationName(DiceCombination combination)
		{
			switch (combination)
			{
				case DiceCombination.Straight_1_6: return "Straight 1-6";
				case DiceCombination.Straight_1_5: return "Straight 1-5";
				case DiceCombination.Straight_2_6: return "Straight 2-6";
				case DiceCombination.ThreeOfAKind: return "Three of a kind";
				case DiceCombination.FourOfAKind: return "Four of a kind";
				case DiceCombination.FiveOfAKind: return "Five of a kind";
				case DiceCombination.SixOfAKind: return "Six of a kind";
				case DiceCombination.SingleOnes: return "Single ones";
				case DiceCombination.SingleFives: return "Single fives";
			}

			return string.Empty;
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
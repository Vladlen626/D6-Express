using System.Linq;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public static class DiceGameUtils
	{
		private class DiceInfo
		{
			public int[] Counts = new int[7];
			public int[] TripleCounts = new int[7];
			public int SingleOnes;
			public int SingleFives;
			public bool IsStraight_1_6;
			public bool IsStraight_1_5;
			public bool IsStraight_2_6;
		}

		private static DiceInfo AnalyzeValues(int[] values)
		{
			var info = new DiceInfo();

			if (values == null || values.Length == 0)
				return info;

			foreach (var value in values)
			{
				if (value >= 1 && value <= 6)
					info.Counts[value]++;
			}

			var sorted = values.OrderBy(v => v).ToArray();

			info.IsStraight_1_6 = (values.Length == 6 && sorted.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6 }));
			info.IsStraight_1_5 = (values.Length == 5 && sorted.SequenceEqual(new[] { 1, 2, 3, 4, 5 }));
			info.IsStraight_2_6 = (values.Length == 5 && sorted.SequenceEqual(new[] { 2, 3, 4, 5, 6 }));

			for (int face = 1; face <= 6; face++)
			{
				if (info.Counts[face] >= 3)
					info.TripleCounts[face] = info.Counts[face];
			}

			info.SingleOnes = System.Math.Max(0, info.Counts[1] - info.TripleCounts[1]);
			info.SingleFives = System.Math.Max(0, info.Counts[5] - info.TripleCounts[5]);

			return info;
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

		/// <summary>
		/// Подсчитать очки. Возвращает -1 если невалидная комбинация.
		/// </summary>
		public static int CalculateScore(int[] values)
		{
			if (values == null || values.Length == 0)
				return -1;

			var info = AnalyzeValues(values);

			if (info.IsStraight_1_6) return 1500;
			if (info.IsStraight_1_5) return 500;
			if (info.IsStraight_2_6) return 750;

			int totalScore = 0;

			for (int face = 1; face <= 6; face++)
			{
				if (info.TripleCounts[face] >= 3)
					totalScore += ScoreNOfKind(face, info.TripleCounts[face]);
			}

			totalScore += info.SingleOnes * 100;
			totalScore += info.SingleFives * 50;

			// Проверка "мёртвых" костей
			foreach (int face in new[] { 2, 3, 4, 6 })
			{
				int unused = info.Counts[face] - info.TripleCounts[face];
				if (unused > 0)
					return -1; // Невалидная комбинация
			}

			return totalScore > 0 ? totalScore : -1;
		}

		/// <summary>
		/// Валидна ли комбинация? (для проверки перед банком)
		/// </summary>
		public static bool IsValidCombo(int[] values)
		{
			return CalculateScore(values) > 0;
		}

		/// <summary>
		/// Есть ли в броске хоть одна очковая кость? (для BUST)
		/// </summary>
		public static bool RollHasAnyScore(int[] values)
		{
			if (values == null || values.Length == 0)
				return false;

			var info = AnalyzeValues(values);

			if (info.IsStraight_1_6 || info.IsStraight_1_5 || info.IsStraight_2_6)
				return true;

			if (values.Contains(1) || values.Contains(5))
				return true;

			for (int face = 1; face <= 6; face++)
			{
				if (info.Counts[face] >= 3)
					return true;
			}

			return false;
		}

		public static int GetWeightedRandomValue(int[] weights)
		{
			float totalWeight = 0f;
			foreach (float weight in weights)
				totalWeight += weight;

			float randomValue = Random.Range(0f, totalWeight);
			float cumulativeWeight = 0f;

			for (int i = 0; i < weights.Length; i++)
			{
				cumulativeWeight += weights[i];
				if (randomValue <= cumulativeWeight)
					return i + 1;
			}

			return 1;
		}
	}
}
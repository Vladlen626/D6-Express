using System.Linq;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Статические утилиты для подсчёта очков и проверки комбинаций
	/// </summary>
	public static class DiceGameUtils
	{
		// ========================================
		// ПОДСЧЁТ ОЧКОВ
		// ========================================

		/// <summary>
		/// Подсчитать очки за набор костей по правилам из документа
		/// </summary>
		public static int CalculateScore(int[] values)
		{
			if (values == null || values.Length == 0)
				return 0;

			int totalScore = 0;
			var counts = new int[7]; // индексы 1-6

			// Считаем количество каждого значения
			foreach (var value in values)
			{
				if (value >= 1 && value <= 6)
					counts[value]++;
			}

			// === СПЕЦИАЛЬНЫЕ КОМБИНАЦИИ ===

			// Стрейт (1-2-3-4-5-6) = 1500
			if (values.Length == 6 && counts.Skip(1).All(c => c == 1))
			{
				return 1500;
			}

			// 3 пары = 750
			int pairCount = counts.Skip(1).Count(c => c == 2);
			if (pairCount == 3)
			{
				return 750;
			}

			// === THREE OF A KIND ===
			for (int face = 1; face <= 6; face++)
			{
				if (counts[face] >= 3)
				{
					if (face == 1)
					{
						// 1-1-1 = 1000
						totalScore += 1000;

						// Каждая дополнительная 1 удваивает
						for (int i = 3; i < counts[face]; i++)
						{
							totalScore *= 2;
						}
					}
					else
					{
						// X-X-X = face * 100
						totalScore += face * 100;

						// Каждая дополнительная кость удваивает
						for (int i = 3; i < counts[face]; i++)
						{
							totalScore *= 2;
						}
					}

					counts[face] = 0; // Убираем использованные кости
				}
			}

			// === ОДИНОЧНЫЕ 1 и 5 ===
			totalScore += counts[1] * 100; // Каждая 1 = 100
			totalScore += counts[5] * 50; // Каждая 5 = 50

			return totalScore;
		}

		// ========================================
		// ПРОВЕРКА КОМБИНАЦИЙ
		// ========================================

		/// <summary>
		/// Проверка на BUST (нет ни одной очковой кости)
		/// </summary>
		public static bool IsBust(int[] values)
		{
			if (values == null || values.Length == 0)
				return true;

			return !HasValidCombo(values);
		}

		/// <summary>
		/// Есть ли хоть одна валидная комбинация?
		/// </summary>
		public static bool HasValidCombo(int[] values)
		{
			if (values == null || values.Length == 0)
				return false;

			var counts = new int[7];
			foreach (var value in values)
			{
				if (value >= 1 && value <= 6)
					counts[value]++;
			}

			// Стрейт 1-2-3-4-5-6
			if (values.Length == 6 && counts.Skip(1).All(c => c == 1))
				return true;

			// 3 пары
			int pairCount = counts.Skip(1).Count(c => c == 2);
			if (pairCount == 3)
				return true;

			// Three of a kind любого значения
			if (counts.Any(c => c >= 3))
				return true;

			// Хотя бы одна 1 или 5
			if (counts[1] > 0 || counts[5] > 0)
				return true;

			return false;
		}

		/// <summary>
		/// Проверка на стрейт (1-2-3-4-5-6)
		/// </summary>
		public static bool HasStraight(int[] values)
		{
			if (values == null || values.Length != 6)
				return false;

			return values.OrderBy(v => v).SequenceEqual(new[] { 1, 2, 3, 4, 5, 6 });
		}

		/// <summary>
		/// Проверка на 3 пары
		/// </summary>
		public static bool HasThreePairs(int[] values)
		{
			if (values == null || values.Length != 6)
				return false;

			var counts = new int[7];
			foreach (var value in values)
			{
				if (value >= 1 && value <= 6)
					counts[value]++;
			}

			int pairCount = counts.Skip(1).Count(c => c == 2);
			return pairCount == 3;
		}

		/// <summary>
		/// Проверка на Three of a Kind
		/// </summary>
		public static bool HasThreeOfKind(int[] values)
		{
			if (values == null || values.Length < 3)
				return false;

			var counts = new int[7];
			foreach (var value in values)
			{
				if (value >= 1 && value <= 6)
					counts[value]++;
			}

			return counts.Any(c => c >= 3);
		}

		// ========================================
		// ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
		// ========================================

		/// <summary>
		/// Получить название комбинации для отображения
		/// </summary>
		public static string GetComboName(int[] values)
		{
			if (values == null || values.Length == 0)
				return "No Combo";

			if (HasStraight(values))
				return "Straight (1500)";

			if (HasThreePairs(values))
				return "Three Pairs (750)";

			if (HasThreeOfKind(values))
			{
				var counts = new int[7];
				foreach (var value in values)
				{
					if (value >= 1 && value <= 6)
						counts[value]++;
				}

				for (int face = 1; face <= 6; face++)
				{
					if (counts[face] >= 3)
					{
						int score = face == 1 ? 1000 : face * 100;
						return $"Three {face}'s ({score})";
					}
				}
			}

			int ones = values.Count(v => v == 1);
			int fives = values.Count(v => v == 5);

			if (ones > 0 && fives > 0)
				return $"{ones}×1 + {fives}×5 ({ones * 100 + fives * 50})";

			if (ones > 0)
				return $"{ones}×1 ({ones * 100})";

			if (fives > 0)
				return $"{fives}×5 ({fives * 50})";

			return "No Combo";
		}
		
		public static int GetWeightedRandomValue(float[] weights)
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
	}
}
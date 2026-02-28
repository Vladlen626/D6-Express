using UnityEngine;

namespace _Main.Scripts.Dice
{
	public static class DiceGameUtils
	{
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

		public static int BaseScoreSetup(DiceCombination diceCombination, int face)
		{
			switch (diceCombination)
			{
				case DiceCombination.Straight_1_6:
					return 1500;

				case DiceCombination.Straight_1_5:
					return 500;

				case DiceCombination.Straight_2_6:
					return 750;

				case DiceCombination.ThreeOfAKind:
					return ScoreNOfKind(face, 3);
				case DiceCombination.FourOfAKind:
					return ScoreNOfKind(face, 4);
				case DiceCombination.FiveOfAKind:
					return ScoreNOfKind(face, 5);
				case DiceCombination.SixOfAKind:
					return ScoreNOfKind(face, 6);

				case DiceCombination.SingleOnes:
					return 100;

				case DiceCombination.SingleFives:
					return 50;

				default:
					return 0;
			}
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

		public static int[] GetDiceValues(DiceModel[] dice)
		{
			int[] values = new int[dice.Length];
			for (int i = 0; i < dice.Length; i++)
			{
				values[i] = dice[i].CurrentValue;
			}

			return values;
		}

		public static bool IsDiceBanked(DiceModel dice, TableModel table)
		{
			if (dice == null || table == null)
			{
				return false;
			}

			var position = dice.CurrentPosition;
			if (!position)
			{
				return false;
			}

			return table.IsBankedPosition(position);
		}
	}
}

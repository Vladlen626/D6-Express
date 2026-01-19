using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class DiceCombinationEntry
	{
		public DiceCombination Combination;
		public int Face;
		public int Count;

		public int BaseScore;

		public int Multiplier = 1;

		public int FinalScore => Mathf.RoundToInt(BaseScore * Multiplier);
	}
}
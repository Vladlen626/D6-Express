using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class DiceCombinationEntry
	{
		/// <summary>String identifier (rule id + face/count where applicable) to support runtime overrides.</summary>
		public string Id;

		/// <summary>Human friendly display name; falls back to rule or enum name when null.</summary>
		public string DisplayName;

		public DiceCombination Combination;
		public int Face;
		public int Count;
		public int BaseScore;
		public int Multiplier = 1;

		public int FinalScore => Mathf.RoundToInt(BaseScore * Multiplier);
	}
}

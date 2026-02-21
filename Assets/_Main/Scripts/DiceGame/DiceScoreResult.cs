using System.Collections.Generic;

namespace _Main.Scripts.Dice
{
	public struct DiceCombinationResult
	{
		public List<DiceCombinationEntry> Combinations;

		/// <summary>Remaining dice counts by face (1..6) after combination consumption.</summary>
		public int[] RemainingCounts;
	}
}

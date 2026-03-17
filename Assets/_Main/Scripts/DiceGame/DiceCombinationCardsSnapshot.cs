using System;

namespace _Main.Scripts.Dice
{
	public readonly struct DiceCombinationCardEntry
	{
		public string Key { get; }
		public string DisplayName { get; }
		public int Score { get; }
		public int[] Faces { get; }

		public DiceCombinationCardEntry(
			string key,
			string displayName,
			int score,
			int[] faces)
		{
			Key = key ?? string.Empty;
			DisplayName = displayName ?? string.Empty;
			Score = score;
			Faces = faces ?? Array.Empty<int>();
		}
	}

	public readonly struct DiceCombinationCardsSnapshot
	{
		public static DiceCombinationCardsSnapshot Empty => new(Array.Empty<DiceCombinationCardEntry>());

		public DiceCombinationCardEntry[] Entries { get; }
		public bool HasEntries => Entries.Length > 0;

		public DiceCombinationCardsSnapshot(DiceCombinationCardEntry[] entries)
		{
			Entries = entries ?? Array.Empty<DiceCombinationCardEntry>();
		}
	}
}

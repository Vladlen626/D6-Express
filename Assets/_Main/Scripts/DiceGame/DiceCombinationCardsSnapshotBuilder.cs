using System;
using System.Buffers;

namespace _Main.Scripts.Dice
{
	public static class DiceCombinationCardsSnapshotBuilder
	{
		private static readonly int[][][] RepeatedFacesCache = BuildRepeatedFacesCache();
		private static readonly int[][][] StraightFacesCache = BuildStraightFacesCache();

		public static DiceCombinationCardsSnapshot Build(
			DiceCombinationResult combinationResult,
			DiceScoringService scoringService)
		{
			if (combinationResult.Combinations == null || combinationResult.Combinations.Count == 0)
			{
				return DiceCombinationCardsSnapshot.Empty;
			}

			var combinations = combinationResult.Combinations;
			var aggregatedEntries = ArrayPool<AggregatedEntry>.Shared.Rent(combinations.Count);
			var aggregatedCount = 0;

			try
			{
				for (int i = 0; i < combinations.Count; i++)
				{
					var entry = combinations[i];
					var entryScore = entry.FinalScore;
					if (entryScore <= 0)
					{
						continue;
					}

					var aggregatedIndex = FindAggregatedIndex(aggregatedEntries, aggregatedCount, entry);
					if (aggregatedIndex < 0)
					{
						aggregatedEntries[aggregatedCount] = new AggregatedEntry(
							ResolveKey(entry),
							entry.Id,
							entry.Combination,
							entry.Face,
							entry.Count,
							ResolveDisplayName(entry, scoringService),
							BuildFaces(entry),
							entryScore);
						aggregatedCount++;
						continue;
					}

					aggregatedEntries[aggregatedIndex].Score += entryScore;
				}

				if (aggregatedCount == 0)
				{
					return DiceCombinationCardsSnapshot.Empty;
				}

				var snapshotEntries = new DiceCombinationCardEntry[aggregatedCount];
				for (int i = 0; i < aggregatedCount; i++)
				{
					var entry = aggregatedEntries[i];
					snapshotEntries[i] = new DiceCombinationCardEntry(
						entry.Key,
						entry.DisplayName,
						entry.Score,
						entry.Faces);
				}

				return new DiceCombinationCardsSnapshot(snapshotEntries);
			}
			finally
			{
				Array.Clear(aggregatedEntries, 0, aggregatedCount);
				ArrayPool<AggregatedEntry>.Shared.Return(aggregatedEntries);
			}
		}

		private static int FindAggregatedIndex(
			AggregatedEntry[] aggregatedEntries,
			int aggregatedCount,
			DiceCombinationEntry sourceEntry)
		{
			var sourceHasId = !string.IsNullOrEmpty(sourceEntry.Id);

			for (int i = 0; i < aggregatedCount; i++)
			{
				var aggregated = aggregatedEntries[i];
				if (sourceHasId)
				{
					if (aggregated.HasId && string.Equals(aggregated.Id, sourceEntry.Id, StringComparison.Ordinal))
					{
						return i;
					}
				}
				else if (!aggregated.HasId &&
				         aggregated.Combination == sourceEntry.Combination &&
				         aggregated.Face == sourceEntry.Face &&
				         aggregated.Count == sourceEntry.Count)
				{
					return i;
				}
			}

			return -1;
		}

		private static string ResolveDisplayName(DiceCombinationEntry entry, DiceScoringService scoringService)
		{
			if (!string.IsNullOrWhiteSpace(entry.DisplayName))
			{
				return entry.DisplayName;
			}

			if (scoringService == null)
			{
				throw new InvalidOperationException(
					"[DiceCombinationCardsSnapshotBuilder] Scoring service is required to resolve combination names.");
			}

			return scoringService.GetDisplayName(entry.Id, entry.Combination);
		}

		private static string ResolveKey(DiceCombinationEntry entry)
		{
			if (!string.IsNullOrEmpty(entry.Id))
			{
				return entry.Id;
			}

			return $"{entry.Combination}:{entry.Face}:{entry.Count}";
		}

		private static int[] BuildFaces(DiceCombinationEntry entry)
		{
			switch (entry.Combination)
			{
				case DiceCombination.Straight_1_6:
					return BuildStraightFaces(1, 6);
				case DiceCombination.Straight_1_5:
					return BuildStraightFaces(1, 5);
				case DiceCombination.Straight_2_6:
					return BuildStraightFaces(2, 5);
				case DiceCombination.StraightLength4:
					return BuildStraightFaces(entry.Face, entry.Count > 0 ? entry.Count : 4);
				case DiceCombination.StraightLength5:
					return BuildStraightFaces(entry.Face, entry.Count > 0 ? entry.Count : 5);
				case DiceCombination.StraightLength6:
					return BuildStraightFaces(entry.Face, entry.Count > 0 ? entry.Count : 6);
				default:
					return BuildRepeatedFaces(entry.Face, entry.Count);
			}
		}

		private static int[] BuildRepeatedFaces(int face, int count)
		{
			if (count <= 0)
			{
				return Array.Empty<int>();
			}

			if (face < 1 || face > 6)
			{
				throw new InvalidOperationException($"Dice face '{face}' is out of range [1..6].");
			}

			if (count <= 6)
			{
				return RepeatedFacesCache[face - 1][count - 1];
			}

			var faces = new int[count];
			for (int i = 0; i < count; i++)
			{
				faces[i] = face;
			}

			return faces;
		}

		private static int[] BuildStraightFaces(int startFace, int count)
		{
			if (count <= 0)
			{
				return Array.Empty<int>();
			}

			if (startFace < 1 || startFace > 6)
			{
				throw new InvalidOperationException($"Straight start face '{startFace}' is out of range [1..6].");
			}

			var endFace = startFace + count - 1;
			if (endFace > 6)
			{
				throw new InvalidOperationException(
					$"Straight faces are out of range: start={startFace}, count={count}, end={endFace}.");
			}

			return StraightFacesCache[startFace - 1][count - 1];
		}

		private static int[][][] BuildRepeatedFacesCache()
		{
			var cache = new int[6][][];
			for (int faceIndex = 0; faceIndex < 6; faceIndex++)
			{
				cache[faceIndex] = new int[6][];
				for (int countIndex = 0; countIndex < 6; countIndex++)
				{
					var count = countIndex + 1;
					var faces = new int[count];
					for (int i = 0; i < count; i++)
					{
						faces[i] = faceIndex + 1;
					}

					cache[faceIndex][countIndex] = faces;
				}
			}

			return cache;
		}

		private static int[][][] BuildStraightFacesCache()
		{
			var cache = new int[6][][];
			for (int startFaceIndex = 0; startFaceIndex < 6; startFaceIndex++)
			{
				cache[startFaceIndex] = new int[6][];
				var startFace = startFaceIndex + 1;

				for (int countIndex = 0; countIndex < 6; countIndex++)
				{
					var count = countIndex + 1;
					var endFace = startFace + count - 1;
					if (endFace > 6)
					{
						continue;
					}

					var faces = new int[count];
					for (int i = 0; i < count; i++)
					{
						faces[i] = startFace + i;
					}

					cache[startFaceIndex][countIndex] = faces;
				}
			}

			return cache;
		}

		private struct AggregatedEntry
		{
			public string Key;
			public string Id;
			public DiceCombination Combination;
			public int Face;
			public int Count;
			public bool HasId;
			public string DisplayName;
			public int[] Faces;
			public int Score;

			public AggregatedEntry(
				string key,
				string id,
				DiceCombination combination,
				int face,
				int count,
				string displayName,
				int[] faces,
				int score)
			{
				Key = key;
				Id = id;
				Combination = combination;
				Face = face;
				Count = count;
				HasId = !string.IsNullOrEmpty(id);
				DisplayName = displayName;
				Faces = faces;
				Score = score;
			}
		}
	}
}

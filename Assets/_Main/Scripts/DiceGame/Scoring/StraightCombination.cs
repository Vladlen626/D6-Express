using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	[Serializable]
	public class StraightBaseScoreEntry
	{
		public int Length = 5;
		public int BaseScore = 500;
	}

	[Serializable]
	public class StraightDefaults
	{
		public int MinLen = 5;
		public int MaxLen = 6;
		public int ScoreBonus = 0;
	}

	[Serializable]
	public class StraightConstraints
	{
		public int MinLenMin = 4;
		public int MaxLenMax = 6;
	}

	[Serializable]
	public class StraightUpgradeOutcome
	{
		public int Face = 1;
		public int DeltaMinLen = 0;
		public int DeltaMaxLen = 0;
		public int DeltaScoreBonus = 0;
	}

	[Serializable]
	public class StraightUpgradeConfig
	{
		public float Chance = 0.15f;
		public bool Debug;
		public bool AllowInventorySelection;
		public StraightUpgradeOutcome[] Outcomes;
		public UpgradeVisualPolarity VisualPolarity = new UpgradeVisualPolarity();
	}

	[Serializable]
	public class StraightConfig
	{
		public StraightBaseScoreEntry[] BaseScores;
		public StraightDefaults Defaults;
		public StraightUpgradeConfig Upgrade;
		public StraightConstraints Constraints;
	}

	[Serializable]
	public class StraightRuntimeState
	{
		public int MinLen;
		public int MaxLen;
		public int ScoreBonus;

		public StraightRuntimeState()
		{
		}

		public StraightRuntimeState(StraightDefaults defaults)
		{
			if (defaults == null)
			{
				MinLen = 5;
				MaxLen = 6;
				ScoreBonus = 0;
				return;
			}

			MinLen = defaults.MinLen;
			MaxLen = defaults.MaxLen;
			ScoreBonus = defaults.ScoreBonus;
		}

		public StraightRuntimeState Clone()
		{
			return new StraightRuntimeState
			{
				MinLen = MinLen,
				MaxLen = MaxLen,
				ScoreBonus = ScoreBonus
			};
		}
	}

	public struct StraightMatch
	{
		public int Length;
		public int StartFace;
		public int[] Faces;
	}

	/// <summary>
	/// Encapsulates straight validation, scoring, and clamping logic.
	/// </summary>
	public class StraightCombination
	{
		private readonly Dictionary<int, int> baseScoresByLength = new();
		private readonly StraightConstraints constraints;
		private readonly Action<string> log;

		public StraightRuntimeState State { get; private set; }
		public bool DebugLogging { get; set; }

		public StraightCombination(
			StraightConfig config,
			StraightRuntimeState initialState,
			Action<string> logAction = null)
		{
			constraints = config?.Constraints ?? new StraightConstraints();
			log = logAction;

			var baseScores = config?.BaseScores;
			if (baseScores != null)
			{
				foreach (var entry in baseScores)
				{
					if (entry == null)
					{
						continue;
					}
					baseScoresByLength[entry.Length] = Mathf.Max(0, entry.BaseScore);
				}
			}

			if (baseScoresByLength.Count == 0)
			{
				// Fallback defaults
				baseScoresByLength[4] = 400;
				baseScoresByLength[5] = 750;
				baseScoresByLength[6] = 1500;
			}

			State = initialState?.Clone() ?? new StraightRuntimeState(config?.Defaults);
			ClampAndValidate(logClamp: false);
		}

		public int MinLen => State.MinLen;
		public int MaxLen => State.MaxLen;
		public int ScoreBonus => State.ScoreBonus;

		public StraightRuntimeState Snapshot() => State.Clone();

		public void ResetToDefaults(StraightDefaults defaults)
		{
			State = new StraightRuntimeState(defaults);
			ClampAndValidate(logClamp: false);
		}

		public void LoadState(StraightRuntimeState newState, bool logClamp = false)
		{
			State = newState?.Clone() ?? new StraightRuntimeState();
			ClampAndValidate(logClamp);
		}

		public void Adjust(StraightUpgradeOutcome outcome)
		{
			if (outcome == null)
			{
				return;
			}

			var before = Snapshot();

			State.MinLen += outcome.DeltaMinLen;
			State.MaxLen += outcome.DeltaMaxLen;
			State.ScoreBonus += outcome.DeltaScoreBonus;

			ClampAndValidate(logClamp: true, before);
		}

		public int GetBaseScore(int length)
		{
			return baseScoresByLength.TryGetValue(length, out var score) ? score : 0;
		}

		public int GetScore(int length)
		{
			return GetBaseScore(length) + State.ScoreBonus;
		}

		/// <summary>
		/// Try to consume one straight from remaining counts. Consumes one of each face used when successful.
		/// </summary>
		public bool TryConsumeStraight(int[] remaining, out StraightMatch match)
		{
			match = default;
			if (remaining == null)
			{
				return false;
			}

			int bestLength = 0;
			int bestStart = 0;

			for (int start = 1; start <= 6; start++)
			{
				if (remaining[start] <= 0)
				{
					continue;
				}

				int length = 0;
				for (int face = start; face <= 6 && remaining[face] > 0; face++)
				{
					length++;
					if (length >= MaxLen)
					{
						break;
					}
				}

				// Only cap at MaxLen; never inflate lengths below MinLen.
				var candidateLength = Mathf.Min(length, MaxLen);
				if (candidateLength < MinLen)
				{
					continue;
				}

				if (candidateLength > bestLength)
				{
					bestLength = candidateLength;
					bestStart = start;
				}
			}

			if (bestLength <= 0)
			{
				return false;
			}

			var faces = Enumerable.Range(bestStart, bestLength).ToArray();
			foreach (var face in faces)
			{
				remaining[face] = Mathf.Max(0, remaining[face] - 1);
			}

			match = new StraightMatch
			{
				Length = bestLength,
				StartFace = bestStart,
				Faces = faces
			};

			return true;
		}

		public void ClampAndValidate(bool logClamp = false, StraightRuntimeState before = null)
		{
			before ??= Snapshot();

			State.MinLen = Mathf.Clamp(State.MinLen, constraints.MinLenMin, constraints.MaxLenMax);
			State.MaxLen = Mathf.Clamp(State.MaxLen, State.MinLen, constraints.MaxLenMax);

			if (!DebugLogging || !logClamp)
			{
				return;
			}

			if (before.MinLen != State.MinLen || before.MaxLen != State.MaxLen || before.ScoreBonus != State.ScoreBonus)
			{
				log?.Invoke($"[Straight] Clamp applied. Before: min={before.MinLen}, max={before.MaxLen}, bonus={before.ScoreBonus} | After: min={State.MinLen}, max={State.MaxLen}, bonus={State.ScoreBonus}");
			}
		}
	}
}

using System;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public static class ScoringUtils
	{
		public static DiceCombination MapToCombination(string ruleId, int count = 0)
		{
			return ruleId switch
			{
				"straight_1_6" => DiceCombination.Straight_1_6,
				"straight_1_5" => DiceCombination.Straight_1_5,
				"straight_2_6" => DiceCombination.Straight_2_6,
				"straight_len_4" => DiceCombination.StraightLength4,
				"straight_len_5" => DiceCombination.StraightLength5,
				"straight_len_6" => DiceCombination.StraightLength6,
				"ofakind" when count == 3 => DiceCombination.ThreeOfAKind,
				"ofakind" when count == 4 => DiceCombination.FourOfAKind,
				"ofakind" when count == 5 => DiceCombination.FiveOfAKind,
				"ofakind" when count >= 6 => DiceCombination.SixOfAKind,
				"single_ones" => DiceCombination.SingleOnes,
				"single_fives" => DiceCombination.SingleFives,
				_ => DiceCombination.None
			};
		}

		public static int ComputeOfAKindScore(ComboRuleDefinition rule, int face, int count)
		{
			if (rule == null)
			{
				throw new ArgumentNullException(nameof(rule));
			}

			// If caller set BaseScore explicitly, honour it with optional scaling rules.
			if (rule.BaseScore > 0 || rule.PerFaceScaling)
			{
				int score = rule.BaseScore;
				if (rule.PerFaceScaling)
				{
					score *= face;
				}

				if (rule.DoublePerExtraAboveMin)
				{
					score = ApplyOfAKindCountScaling(score, count, rule.MinCount);
				}

				return score;
			}

			// Legacy behaviour: 1s are 1000, others face * BaseScorePerPip, doubling per extra.
			int baseScore = (face == 1) ? rule.BaseScoreForOne : face * rule.BaseScorePerPip;
			if (rule.DoublePerExtraAboveMin)
			{
				baseScore = ApplyOfAKindCountScaling(baseScore, count, rule.MinCount);
			}

			return baseScore;
		}

		public static int ApplyOfAKindCountScaling(int score, int count, int baseMinCount)
		{
			if (score <= 0)
			{
				return 0;
			}

			var safeBaseMin = Mathf.Max(1, baseMinCount);
			var delta = count - safeBaseMin;
			if (delta == 0)
			{
				return score;
			}

			var factor = 1 << Mathf.Abs(delta);
			if (delta > 0)
			{
				return score * factor;
			}

			return score / factor;
		}

		public static bool IsStraightRuleMatch(ComboRuleDefinition rule, StraightMatch match)
		{
			if (rule == null)
			{
				return false;
			}

			var faces = rule.Faces;
			if (faces == null || faces.Length != match.Length)
			{
				return false;
			}

			for (int i = 0; i < faces.Length; i++)
			{
				if (faces[i] != match.StartFace + i)
				{
					return false;
				}
			}

			return true;
		}
	}
}

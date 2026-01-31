using System;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	[Serializable]
	public class ComboRuleDefinition
	{
		public string Id;
		public string DisplayName;
		public ComboRuleType RuleType;

		[Tooltip("For straights: faces that must appear once each. For singles/of-a-kind: restrict to these faces, leave empty to allow any.")]
		public int[] Faces;

		[Tooltip("Minimum dice needed to trigger the rule (e.g., 3 for three-of-a-kind).")]
		public int MinCount = 1;

		[Tooltip("Maximum dice the rule can consume in a single match. Use 0 or negative for no cap.")]
		public int MaxCount = 0;

		[Tooltip("Base score used by the rule. For singles, applied per die. For of-a-kind, combined with other toggles below.")]
		public int BaseScore = 0;

		[Tooltip("Allow applying the rule multiple times in one evaluation if dice remain.")]
		public bool Repeatable = true;

		[Tooltip("If true, base score is multiplied by face value (e.g., 100 * face).")]
		public bool PerFaceScaling = false;

		[Tooltip("For of-a-kind: when true, each extra die above MinCount doubles the score.")]
		public bool DoublePerExtraAboveMin = false;

		[Tooltip("For of-a-kind: score to use when face == 1 (legacy behaviour).")]
		public int BaseScoreForOne = 1000;

		[Tooltip("For of-a-kind: base multiplier per pip when face != 1 (legacy 100).")]
		public int BaseScorePerPip = 100;

		[Tooltip("Enable/disable the rule without removing it.")]
		public bool Enabled = true;
	}
}

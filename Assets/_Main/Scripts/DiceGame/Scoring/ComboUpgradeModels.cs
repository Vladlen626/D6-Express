using System;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	[Serializable]
	public class ComboUpgradeOutcome
	{
		public int Face = 1;
		public int DeltaMin = 0;
		public int DeltaMax = 0;
		public int DeltaScoreBonus = 0;
	}

	[Serializable]
	public class ComboUpgradeConstraints
	{
		public int MinLowerBound = 1;
		public int MaxUpperBound = 6;
	}

	[Serializable]
	public class ComboUpgradeConfig
	{
		public string ComboId; // e.g., "straight", "ofakind"
		public float Chance = 0.15f;
		public bool Debug = false;
		public ComboUpgradeOutcome[] Outcomes;
		public ComboUpgradeConstraints Constraints = new ComboUpgradeConstraints();
	}

	[Serializable]
	public class ComboUpgradeConfigRoot
	{
		public ComboUpgradeConfig[] Upgrades;
	}

	[Serializable]
	public class ComboUpgradeState
	{
		public int Min;
		public int Max;
		public int ScoreBonus;

		public ComboUpgradeState Clone()
		{
			return new ComboUpgradeState { Min = Min, Max = Max, ScoreBonus = ScoreBonus };
		}
	}
}

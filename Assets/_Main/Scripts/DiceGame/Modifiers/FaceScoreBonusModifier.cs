using System;
using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	public class FaceScoreBonusModifier : IOnPassModifier, IModifierUiConfigProvider, IModifierApplyResultProvider
	{
		public readonly int faceValue;
		public readonly int bonusPerScoringDie;
		public string UiConfigId { get; }
		public bool LastApplyHadEffect { get; private set; }

		public FaceScoreBonusModifier(int faceValue, int bonusPerScoringDie, string uiConfigId = null)
		{
			if (faceValue < 1 || faceValue > 6)
			{
				throw new ArgumentOutOfRangeException(nameof(faceValue), faceValue, "Face value must be in [1..6].");
			}

			this.faceValue = faceValue;
			this.bonusPerScoringDie = bonusPerScoringDie;
			UiConfigId = uiConfigId;
		}

		public UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			LastApplyHadEffect = false;
			var combinations = modifierContext.CombinationResult.Combinations;
			for (int i = 0; i < combinations.Count; i++)
			{
				var entry = combinations[i];
				var scoringDiceCount = GetScoringDiceCount(entry, faceValue);
				if (scoringDiceCount <= 0)
				{
					continue;
				}

				entry.BaseScore += scoringDiceCount * bonusPerScoringDie;
				LastApplyHadEffect = true;
			}

			return UniTask.CompletedTask;
		}

		private static int GetScoringDiceCount(DiceCombinationEntry entry, int trackedFace)
		{
			switch (entry.Combination)
			{
				case DiceCombination.SingleOnes:
					return trackedFace == 1 ? entry.Count : 0;

				case DiceCombination.SingleFives:
					return trackedFace == 5 ? entry.Count : 0;

				case DiceCombination.ThreeOfAKind:
				case DiceCombination.FourOfAKind:
				case DiceCombination.FiveOfAKind:
				case DiceCombination.SixOfAKind:
					return entry.Face == trackedFace ? entry.Count : 0;

				case DiceCombination.Straight_1_5:
					return trackedFace >= 1 && trackedFace <= 5 ? 1 : 0;

				case DiceCombination.Straight_2_6:
					return trackedFace >= 2 && trackedFace <= 6 ? 1 : 0;

				case DiceCombination.Straight_1_6:
					return trackedFace >= 1 && trackedFace <= 6 ? 1 : 0;

				case DiceCombination.StraightLength4:
				case DiceCombination.StraightLength5:
				case DiceCombination.StraightLength6:
					var minFace = entry.Face;
					var maxFace = entry.Face + entry.Count - 1;
					return trackedFace >= minFace && trackedFace <= maxFace ? 1 : 0;
			}

			return 0;
		}
	}
}

using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	public class FlatComboBonusModifier : IOnPassModifier, IModifierUiConfigProvider
	{
		public readonly DiceCombination combination;
		public readonly int bonusScore;
		public string UiConfigId { get; }

		private readonly bool matchStraightFamily;

		public FlatComboBonusModifier(
			DiceCombination combination,
			int bonusScore,
			string uiConfigId = null,
			bool matchStraightFamily = false)
		{
			this.combination = combination;
			this.bonusScore = bonusScore;
			UiConfigId = uiConfigId;
			this.matchStraightFamily = matchStraightFamily;
		}

		public UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			var combinations = modifierContext.CombinationResult.Combinations;
			for (int i = 0; i < combinations.Count; i++)
			{
				var entry = combinations[i];
				if (!IsCombinationMatch(entry.Combination))
				{
					continue;
				}

				entry.BaseScore += bonusScore;
			}

			return UniTask.CompletedTask;
		}

		private bool IsCombinationMatch(DiceCombination target)
		{
			if (target == combination)
			{
				return true;
			}

			if (!matchStraightFamily)
			{
				return false;
			}

			return IsStraight(target) && IsStraight(combination);
		}

		private static bool IsStraight(DiceCombination combinationToCheck)
		{
			switch (combinationToCheck)
			{
				case DiceCombination.Straight_1_6:
				case DiceCombination.Straight_1_5:
				case DiceCombination.Straight_2_6:
				case DiceCombination.StraightLength4:
				case DiceCombination.StraightLength5:
				case DiceCombination.StraightLength6:
					return true;
			}

			return false;
		}
	}
}

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class MultiplyComboModifier : IOnPassModifier, IModifierUiConfigProvider, IModifierApplyResultProvider
	{
		public readonly DiceCombination combination;
		public readonly int deltaMultiplier;
		public string UiConfigId { get; }
		public bool LastApplyHadEffect { get; protected set; }
		private readonly bool matchStraightFamily;

		public MultiplyComboModifier(
			DiceCombination combination,
			int deltaMultiplier = 1,
			string uiConfigId = null,
			bool matchStraightFamily = false)
		{
			this.combination = combination;
			this.deltaMultiplier = deltaMultiplier;
			UiConfigId = uiConfigId;
			this.matchStraightFamily = matchStraightFamily;
		}
		
		public UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			LastApplyHadEffect = false;
			foreach (var diceCombinationEntry in modifierContext.CombinationResult.Combinations)
			{
				if (IsCombinationMatch(diceCombinationEntry.Combination))
				{
					if (CheckDiceCombinationEntry(diceCombinationEntry))
					{
						var oldValue = diceCombinationEntry.Multiplier;
						diceCombinationEntry.Multiplier = Mathf.Max(1, diceCombinationEntry.Multiplier + deltaMultiplier);
						if (diceCombinationEntry.Multiplier != oldValue)
						{
							LastApplyHadEffect = true;
						}
					}
				}
			}

			return UniTask.CompletedTask;
		}

		protected virtual bool IsCombinationMatch(DiceCombination target)
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

		protected virtual bool CheckDiceCombinationEntry(DiceCombinationEntry diceCombinationEntry)
		{
			return true;
		}

		protected static bool IsStraight(DiceCombination combinationToCheck)
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

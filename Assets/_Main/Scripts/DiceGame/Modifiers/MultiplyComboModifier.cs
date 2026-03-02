using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	public class MultiplyComboModifier : IOnPassModifier
	{
		public readonly DiceCombination combination;

		public MultiplyComboModifier(DiceCombination combination)
		{
			this.combination = combination;
		}
		
		public UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			foreach (var diceCombinationEntry in modifierContext.CombinationResult.Combinations)
			{
				if (diceCombinationEntry.Combination == combination)
				{
					if (CheckDiceCombinationEntry(diceCombinationEntry))
					{
						diceCombinationEntry.Multiplier += 1;
					}
				}
			}

			return UniTask.CompletedTask;
		}

		protected virtual bool CheckDiceCombinationEntry(DiceCombinationEntry diceCombinationEntry)
		{
			return true;
		}
	}
}

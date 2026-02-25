using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	public class MultiplyComboModifier : ModifierItemBase, IOnPassModifier
	{
		public readonly DiceCombination combination;

		public MultiplyComboModifier(string id, DiceCombination combination)
			: base(id, id, DiceItemActivationType.Passive)
		{
			this.combination = combination;
		}
		
		public override UniTask ModifyValues(DiceModifierContext modifierContext)
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

using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	public class MultiplyComboModifier : IOnPassModifier
	{
		private readonly DiceCombination combination;

		public MultiplyComboModifier(DiceCombination combination)
		{
			this.combination = combination;
		}
		
		public UniTask ModifyValues(DiceCombinationResult diceCombinationResult)
		{
			foreach (var diceCombinationEntry in diceCombinationResult.Combinations)
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
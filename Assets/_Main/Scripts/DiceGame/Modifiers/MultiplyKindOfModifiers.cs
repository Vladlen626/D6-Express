namespace _Main.Scripts.Dice
{
	public class MultiplyKindOfModifiers : MultiplyComboModifier
	{
		public readonly int face;
		
		public MultiplyKindOfModifiers(
			DiceCombination combination,
			int face,
			int deltaMultiplier = 1,
			string uiConfigId = null) : base(combination, deltaMultiplier, uiConfigId)
		{
			this.face = face;
		}

		protected override bool CheckDiceCombinationEntry(DiceCombinationEntry diceCombinationEntry)
		{
			return diceCombinationEntry.Face == face;
		}
	}
}

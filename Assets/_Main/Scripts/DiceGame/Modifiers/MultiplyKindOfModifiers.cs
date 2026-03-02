namespace _Main.Scripts.Dice
{
	public class MultiplyKindOfModifiers : MultiplyComboModifier
	{
		public readonly int face;
		
		public MultiplyKindOfModifiers(DiceCombination combination, int face) : base(combination)
		{
			this.face = face;
		}

		protected override bool CheckDiceCombinationEntry(DiceCombinationEntry diceCombinationEntry)
		{
			return diceCombinationEntry.Face == face;
		}
	}
}

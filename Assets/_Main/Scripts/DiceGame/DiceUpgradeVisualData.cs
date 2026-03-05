namespace _Main.Scripts.Dice
{
	public readonly struct DiceUpgradeVisualData
	{
		public readonly string ComboId;
		public readonly string Title;
		public readonly string RolledText;
		public readonly string MinLabel;
		public readonly string MaxLabel;
		public readonly string BonusLabel;
		public readonly string HintText;
		public readonly int RolledFace;
		public readonly int BeforeMin;
		public readonly int BeforeMax;
		public readonly int BeforeBonus;
		public readonly int AfterMin;
		public readonly int AfterMax;
		public readonly int AfterBonus;

		public DiceUpgradeVisualData(
			string comboId,
			string title,
			string rolledText,
			string minLabel,
			string maxLabel,
			string bonusLabel,
			string hintText,
			int rolledFace,
			int beforeMin,
			int beforeMax,
			int beforeBonus,
			int afterMin,
			int afterMax,
			int afterBonus)
		{
			ComboId = comboId;
			Title = title;
			RolledText = rolledText;
			MinLabel = minLabel;
			MaxLabel = maxLabel;
			BonusLabel = bonusLabel;
			HintText = hintText;
			RolledFace = rolledFace;
			BeforeMin = beforeMin;
			BeforeMax = beforeMax;
			BeforeBonus = beforeBonus;
			AfterMin = afterMin;
			AfterMax = afterMax;
			AfterBonus = afterBonus;
		}
	}
}

using System;

namespace _Main.Scripts.Dice
{
	public readonly struct DiceUpgradeRouletteSlotData
	{
		public readonly int Face;
		public readonly string BonusText;

		public DiceUpgradeRouletteSlotData(int face, string bonusText)
		{
			Face = face;
			BonusText = bonusText ?? "0";
		}
	}

	public readonly struct DiceUpgradeVisualData
	{
		public readonly string ComboId;
		public readonly string Title;
		public readonly string RolledText;
		public readonly string MinLabel;
		public readonly string MaxLabel;
		public readonly string BonusLabel;
		public readonly string HintText;
		public readonly string StopHintText;
		public readonly int RolledFace;
		public readonly int BeforeMin;
		public readonly int BeforeMax;
		public readonly int BeforeBonus;
		public readonly int AfterMin;
		public readonly int AfterMax;
		public readonly int AfterBonus;
		public readonly DiceUpgradeRouletteSlotData[] RouletteSlots;

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
			: this(
				comboId,
				title,
				rolledText,
				minLabel,
				maxLabel,
				bonusLabel,
				hintText,
				string.Empty,
				rolledFace,
				beforeMin,
				beforeMax,
				beforeBonus,
				afterMin,
				afterMax,
				afterBonus,
				null)
		{
		}

		public DiceUpgradeVisualData(
			string comboId,
			string title,
			string rolledText,
			string minLabel,
			string maxLabel,
			string bonusLabel,
			string hintText,
			string stopHintText,
			int rolledFace,
			int beforeMin,
			int beforeMax,
			int beforeBonus,
			int afterMin,
			int afterMax,
			int afterBonus,
			DiceUpgradeRouletteSlotData[] rouletteSlots)
		{
			ComboId = comboId;
			Title = title;
			RolledText = rolledText;
			MinLabel = minLabel;
			MaxLabel = maxLabel;
			BonusLabel = bonusLabel;
			HintText = hintText;
			StopHintText = stopHintText ?? string.Empty;
			RolledFace = rolledFace;
			BeforeMin = beforeMin;
			BeforeMax = beforeMax;
			BeforeBonus = beforeBonus;
			AfterMin = afterMin;
			AfterMax = afterMax;
			AfterBonus = afterBonus;
			RouletteSlots = rouletteSlots ?? Array.Empty<DiceUpgradeRouletteSlotData>();
		}
	}
}

using System;

namespace _Main.Scripts.Dice
{
	public enum ModifierStage
	{
		RoundStart,
		Roll,
		Pass,
		RoundEnd
	}

	public class DiceModifierContext
	{
		public DiceModifierContext(
			DiceCombinationResult combinationResult,
			DiceModel[] dice,
			TableModel table,
			DiceGameModel diceGameModel,
			ModifierStage stage,
			LevelModel levelModel = null)
		{
			CombinationResult = combinationResult;
			Dice = dice ?? Array.Empty<DiceModel>();
			Table = table;
			DiceGameModel = diceGameModel;
			Stage = stage;
			LevelModel = levelModel;
		}

		public DiceCombinationResult CombinationResult { get; }
		public DiceModel[] Dice { get; }
		public TableModel Table { get; }
		public DiceGameModel DiceGameModel { get; }
		public ModifierStage Stage { get; }
		public LevelModel LevelModel { get; }
	}
}

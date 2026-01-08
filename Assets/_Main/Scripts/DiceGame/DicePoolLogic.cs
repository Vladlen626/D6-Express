using System.Linq;

namespace _Main.Scripts.Dice
{
	public class DicePoolLogic
	{
		private readonly DiceGameModel diceGameModel;

		public DicePoolLogic(DiceGameModel diceGameModel)
		{
			this.diceGameModel = diceGameModel;
		}

		public DiceModel[] GetSelected()
		{
			return diceGameModel.GameSelectedDiceModelsList.Where(d => d.IsChosen && !d.IsSaved).ToArray();
		}

		public DiceModel[] GetUnbanked()
		{
			return diceGameModel.GameSelectedDiceModelsList.Where(d => !d.IsSaved).ToArray();
		}

		public DiceModel[] GetBanked()
		{
			return diceGameModel.GameSelectedDiceModelsList.Where(d => d.IsSaved).ToArray();
		}

		public bool HasUnbanked()
		{
			return diceGameModel.GameSelectedDiceModelsList.Any(d => !d.IsSaved);
		}

		public bool AllBanked()
		{
			return diceGameModel.GameSelectedDiceModelsList.All(d => d.IsSaved);
		}

		public void BankSelected()
		{
			var selected = GetSelected();
			foreach (var dice in selected)
			{
				dice.SetSaved(true);
				dice.SetChosen(false);
			}
		}

		public void ResetAll()
		{
			foreach (var dice in diceGameModel.GameSelectedDiceModelsList)
			{
				dice.Reset();
			}
		}
	}
}
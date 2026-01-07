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
			return diceGameModel.DiceModelsList.Where(d => d.IsChosen && !d.IsSaved).ToArray();
		}

		public DiceModel[] GetUnbanked()
		{
			return diceGameModel.DiceModelsList.Where(d => !d.IsSaved).ToArray();
		}

		public DiceModel[] GetBanked()
		{
			return diceGameModel.DiceModelsList.Where(d => d.IsSaved).ToArray();
		}

		public bool HasUnbanked()
		{
			return diceGameModel.DiceModelsList.Any(d => !d.IsSaved);
		}

		public bool AllBanked()
		{
			return diceGameModel.DiceModelsList.All(d => d.IsSaved);
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
			foreach (var dice in diceGameModel.DiceModelsList)
			{
				dice.Reset();
			}
		}
	}
}
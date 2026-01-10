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
			return diceGameModel.CurrentDiceModelList.Where(d => d.IsChosen && !d.IsSaved).ToArray();
		}

		public DiceModel[] GetUnbanked()
		{
			return diceGameModel.CurrentDiceModelList.Where(d => !d.IsSaved).ToArray();
		}

		public DiceModel[] GetBanked()
		{
			return diceGameModel.CurrentDiceModelList.Where(d => d.IsSaved).ToArray();
		}

		public bool HasUnbanked()
		{
			return diceGameModel.CurrentDiceModelList.Any(d => !d.IsSaved);
		}

		public bool AllBanked()
		{
			return diceGameModel.CurrentDiceModelList.All(d => d.IsSaved);
		}

		public void ResetAll()
		{
			foreach (var dice in diceGameModel.CurrentDiceModelList)
			{
				dice.Reset();
			}
		}
	}
}
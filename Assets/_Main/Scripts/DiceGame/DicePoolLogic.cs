using System.Linq;

namespace _Main.Scripts.Dice
{
	public class DicePoolLogic
	{
		private readonly DiceModel[] _diceModels;

		public DicePoolLogic(DiceModel[] diceModels)
		{
			_diceModels = diceModels;
		}

		public DiceModel[] GetSelected()
		{
			return _diceModels.Where(d => d.IsChosen && !d.IsSaved).ToArray();
		}

		public DiceModel[] GetUnbanked()
		{
			return _diceModels.Where(d => !d.IsSaved).ToArray();
		}

		public DiceModel[] GetBanked()
		{
			return _diceModels.Where(d => d.IsSaved).ToArray();
		}

		public bool HasUnbanked()
		{
			return _diceModels.Any(d => !d.IsSaved);
		}

		public bool AllBanked()
		{
			return _diceModels.All(d => d.IsSaved);
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
			foreach (var dice in _diceModels)
			{
				dice.Reset();
			}
		}
	}
}
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

namespace _Main.Scripts.Dice
{
	public class DiceGameBetViewController : IBaseController, IActivatable
	{
		private const int Bet1xMultiplier = 1;
		private const int Bet3xMultiplier = 3;
		private const int Bet5xMultiplier = 5;

		public readonly DiceGameModel diceGameModel;
		public readonly DiceTableView diceTableView;

		public DiceGameBetViewController(DiceGameModel diceGameModel, DiceTableView diceTableView)
		{
			this.diceGameModel = diceGameModel;
			this.diceTableView = diceTableView;
		}
		public void Activate()
		{
			ConfigureBetMode();
		}

		public void Deactivate()
		{
		}

		private void ConfigureBetMode()
		{
			var minBet = diceGameModel.MinBetSize;
			var maxBet = diceGameModel.MaxBetSize;
			var bet1x = minBet * Bet1xMultiplier;
			var bet3x = minBet * Bet3xMultiplier;
			var bet5x = minBet * Bet5xMultiplier;
			diceTableView.SetBetButtonsAmounts(bet1x, bet3x, bet5x, maxBet);

			var hasEnoughForMinBet = maxBet >= minBet;
			diceTableView.ShowBetMultipliers(hasEnoughForMinBet);
			diceTableView.ShowAllInButton(!hasEnoughForMinBet);

			if (!hasEnoughForMinBet)
			{
				diceTableView.SetBetMultiplierButtonsInteractable(false, false, false);
				diceTableView.SetAllInButtonInteractable(maxBet > 0);
				return;
			}

			diceTableView.SetAllInButtonInteractable(false);
			diceTableView.SetBetMultiplierButtonsInteractable(
				maxBet >= bet1x,
				maxBet >= bet3x,
				maxBet >= bet5x);
		}
	}
}

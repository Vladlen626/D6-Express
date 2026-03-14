using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

namespace _Main.Scripts.Dice
{
	public class DiceGameBetController : IBaseController, IActivatable
	{
		private const int Bet1xMultiplier = 1;
		private const int Bet3xMultiplier = 3;
		private const int Bet5xMultiplier = 5;

		public readonly DiceGameModel diceGameModel;
		public readonly DiceTableView diceTableView;

		public DiceGameBetController(DiceGameModel diceGameModel, DiceTableView diceTableView)
		{
			this.diceGameModel = diceGameModel;
			this.diceTableView = diceTableView;
		}
		public void Activate()
		{
			diceTableView.OnBet1xClicked += OnBet1xClickedHandler;
			diceTableView.OnBet3xClicked += OnBet3xClickedHandler;
			diceTableView.OnBet5xClicked += OnBet5xClickedHandler;
			diceTableView.OnAllInClicked += OnAllInClickedHandler;
		}

		public void Deactivate()
		{
			diceTableView.OnBet1xClicked -= OnBet1xClickedHandler;
			diceTableView.OnBet3xClicked -= OnBet3xClickedHandler;
			diceTableView.OnBet5xClicked -= OnBet5xClickedHandler;
			diceTableView.OnAllInClicked -= OnAllInClickedHandler;
		}

		private void OnBet1xClickedHandler()
		{
			TryStartMultiplierBet(Bet1xMultiplier);
		}

		private void OnBet3xClickedHandler()
		{
			TryStartMultiplierBet(Bet3xMultiplier);
		}

		private void OnBet5xClickedHandler()
		{
			TryStartMultiplierBet(Bet5xMultiplier);
		}

		private void OnAllInClickedHandler()
		{
			TryStartBet(diceGameModel.MaxBetSize, true);
		}

		private void TryStartMultiplierBet(int multiplier)
		{
			var betSize = diceGameModel.MinBetSize * multiplier;
			TryStartBet(betSize, false);
		}

		private void TryStartBet(int betSize, bool isAllInBet)
		{
			if (diceGameModel.DiceGameState != DiceGameState.BET)
			{
				return;
			}

			if (betSize <= 0)
			{
				return;
			}

			if (betSize > diceGameModel.MaxBetSize)
			{
				return;
			}

			if (!isAllInBet && betSize < diceGameModel.MinBetSize)
			{
				return;
			}

			diceGameModel.SetBetSize(betSize, isAllInBet);
			diceGameModel.ChangeDiceGameState(DiceGameState.GAME);
		}
	}
}

using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

namespace _Main.Scripts.Dice
{
	public class DiceGameBetViewController : IBaseController, IActivatable
	{
		public readonly DiceGameModel diceGameModel;
		public readonly DiceTableView diceTableView;

		public DiceGameBetViewController(DiceGameModel diceGameModel, DiceTableView diceTableView)
		{
			this.diceGameModel = diceGameModel;
			this.diceTableView = diceTableView;
		}
		public void Activate()
		{
			diceGameModel.OnBetSizeChanged += OnBetSizeChangedHandler;
			
			diceTableView.SetMinBet(diceGameModel.MinBetSize);
			diceTableView.SetMaxBet(diceGameModel.MaxBetSize);
			diceTableView.SetBet(diceGameModel.BetSize);
			OnBetSizeChangedHandler();
		}

		public void Deactivate()
		{
			diceGameModel.OnBetSizeChanged -= OnBetSizeChangedHandler;
		}

		private void OnBetSizeChangedHandler()
		{
			diceTableView.SetCurrentBetText(diceGameModel.BetSize.ToString());
		}
	}
}
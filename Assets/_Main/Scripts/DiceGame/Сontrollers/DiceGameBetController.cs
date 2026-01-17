using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

namespace _Main.Scripts.Dice
{
	public class DiceGameBetController : IBaseController, IActivatable
	{
		public readonly DiceGameModel diceGameModel;
		public readonly DiceTableView diceTableView;
		
		private TableModel tableModel => diceGameModel.tableModel;

		public DiceGameBetController(DiceGameModel diceGameModel, DiceTableView diceTableView)
		{
			this.diceGameModel = diceGameModel;
			this.diceTableView = diceTableView;
		}
		public void Activate()
		{
			diceTableView.OnBetSliderChange += OnBetSliderChangeHandler;
			diceTableView.OnBetClicked += OnBetClickedHandler;
		}

		public void Deactivate()
		{
			diceTableView.OnBetSliderChange -= OnBetSliderChangeHandler;
			diceTableView.OnBetClicked -= OnBetClickedHandler;
		}

		private void OnBetClickedHandler()
		{
			diceGameModel.ChangeDiceGameState(DiceGameState.GAME);
		}

		private void OnBetSliderChangeHandler(int betSize)
		{
			diceGameModel.SetBetSize(betSize);
		}
	}
}
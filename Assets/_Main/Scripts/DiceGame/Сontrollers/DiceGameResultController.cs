using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

namespace _Main.Scripts.Dice
{
	public class DiceGameResultController : IBaseController, IActivatable
	{
		private readonly DiceGameModel diceGameModel;
		private readonly TableModel tableModel;

		public DiceGameResultController(DiceGameModel diceGameModel, TableModel tableModel)
		{
			this.diceGameModel = diceGameModel;
			this.tableModel = tableModel;
		}
		public void Activate()
		{
			tableModel.OnBankedPointsChanged += OnBankedPointsChangedHandler;
			diceGameModel.OnCurrentTurnChanged += OnCurrentTurnChangedHandler;
		}

		public void Deactivate()
		{
			tableModel.OnBankedPointsChanged -= OnBankedPointsChangedHandler;
			diceGameModel.OnCurrentTurnChanged -= OnCurrentTurnChangedHandler;
		}
		private void OnBankedPointsChangedHandler()
		{
			if (tableModel.BankedPoints >= diceGameModel.TargetPoints)
			{
				diceGameModel.SetConditionPassed();
			}
		}
		
		private void OnCurrentTurnChangedHandler()
		{
			if (diceGameModel.CurrentTurn > diceGameModel.MaxTurnCount)
			{
				diceGameModel.SetConditionFailed();
			}
		}
	}
}
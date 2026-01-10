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
			tableModel.OnPlayerBankedPointsChanged += OnPlayerBankedPointsChangedHandler;
			tableModel.OnEnemyBankedPointsChanged += OnEnemyBankedPointsChangedHandler;
		}

		public void Deactivate()
		{
			tableModel.OnPlayerBankedPointsChanged -= OnPlayerBankedPointsChangedHandler;
			tableModel.OnEnemyBankedPointsChanged -= OnEnemyBankedPointsChangedHandler;
		}
		private void OnPlayerBankedPointsChangedHandler()
		{
			if (tableModel.PlayerBankedPoints >= diceGameModel.TargetPoints)
			{
				diceGameModel.SetConditionPassed();
			}
		}
		
		private void OnEnemyBankedPointsChangedHandler()
		{
			if (tableModel.EnemyBankedPoints >= diceGameModel.TargetPoints)
			{
				diceGameModel.SetConditionFailed();
			}
		}
	}
}
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

namespace _Main.Scripts.Dice
{
	public class DiceGameResultController : IBaseController, IActivatable
	{
		private readonly DiceGameModel diceGameModel;
		private TableModel tableModel => diceGameModel.tableModel;

		public DiceGameResultController(DiceGameModel diceGameModel)
		{
			this.diceGameModel = diceGameModel; ;
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
		private void OnPlayerBankedPointsChangedHandler(int oldValue, int newValue)
		{
			if (tableModel.PlayerBankedPoints >= diceGameModel.TargetPoints)
			{
				diceGameModel.SetConditionPassed(
					DiceMatchResultReason.PlayerReachedTarget,
					DiceMatchStage.RoundEnd,
					"banked_points");
			}
		}
		
		private void OnEnemyBankedPointsChangedHandler(int oldValue, int newValue)
		{
			if (tableModel.EnemyBankedPoints >= diceGameModel.TargetPoints)
			{
				diceGameModel.SetConditionFailed(
					DiceMatchResultReason.EnemyReachedTarget,
					DiceMatchStage.RoundEnd,
					"banked_points");
			}
		}
	}
}

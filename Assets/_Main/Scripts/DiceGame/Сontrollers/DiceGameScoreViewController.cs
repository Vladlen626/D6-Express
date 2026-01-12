using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

namespace _Main.Scripts.Dice
{
	public class DiceGameScoreViewController : IBaseController, IActivatable
	{
		private readonly DiceGameModel diceGameModel;
		private readonly TableModel tableModel;
		private readonly DiceTableView diceTableView;

		public DiceGameScoreViewController(TableModel tableModel, DiceTableView diceTableView,
			DiceGameModel diceGameModel)
		{
			this.tableModel = tableModel;
			this.diceTableView = diceTableView;
			this.diceGameModel = diceGameModel;
		}
		public void Activate()
		{
			diceGameModel.OnTargetPointsChanged += OnTargetPointsChangedHandler;
			diceGameModel.OnCurrentTurnChanged += OnCurrentTurnChangedHandler;
			tableModel.OnPlayerBankedPointsChanged += OnPlayerBankedPointsChangedHandler;
			tableModel.OnEnemyBankedPointsChanged += OnEnemyBankedPointsChangedHandler;
			tableModel.OnTurnPointsChanged += OnTurnPointsChangedHandler;
			tableModel.OnPreviewPointsChanged += OnPreviewPointsChangedHandler;

			OnPlayerBankedPointsChangedHandler();
			OnTargetPointsChangedHandler();
			OnTurnPointsChangedHandler();
			OnPreviewPointsChangedHandler();
			OnCurrentTurnChangedHandler();
		}

		public void Deactivate()
		{
			diceGameModel.OnTargetPointsChanged -= OnTargetPointsChangedHandler;
			diceGameModel.OnCurrentTurnChanged -= OnCurrentTurnChangedHandler;
			tableModel.OnPlayerBankedPointsChanged -= OnPlayerBankedPointsChangedHandler;
			tableModel.OnEnemyBankedPointsChanged -= OnEnemyBankedPointsChangedHandler;
			tableModel.OnTurnPointsChanged -= OnTurnPointsChangedHandler;
			tableModel.OnPreviewPointsChanged -= OnPreviewPointsChangedHandler;
		}

		private void OnPlayerBankedPointsChangedHandler()
		{
			diceTableView.SetPlayerBankedPointsText(tableModel.PlayerBankedPoints.ToString());
		}
		
		private void OnEnemyBankedPointsChangedHandler()
		{
			diceTableView.SetEnemyBankedPointsText(tableModel.EnemyBankedPoints.ToString());
		}

		private void OnTargetPointsChangedHandler()
		{
			diceTableView.SetTargetPointsText(diceGameModel.TargetPoints);
		}

		private void OnCurrentTurnChangedHandler()
		{
			diceTableView.SwitchTurn(diceGameModel.IsPlayerTurn);
			diceTableView.SetTurnText(diceGameModel.CurrentTurn);
		}

		private void OnTurnPointsChangedHandler()
		{
			diceTableView.SetCurrentPointsText(tableModel.TurnPoints.ToString());
		}
		
		private void OnPreviewPointsChangedHandler()
		{
			diceTableView.SetPreviewPointsText(tableModel.PreviewPoints.ToString());
		}
	}
}
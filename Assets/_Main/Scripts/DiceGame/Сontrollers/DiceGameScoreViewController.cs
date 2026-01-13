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

			OnPlayerBankedPointsChangedHandler(0, tableModel.PlayerBankedPoints);
			OnTargetPointsChangedHandler(0, diceGameModel.TargetPoints);
			OnTurnPointsChangedHandler(0, tableModel.TurnPoints);
			OnPreviewPointsChangedHandler(0, tableModel.PreviewPoints);
			OnCurrentTurnChangedHandler(0, diceGameModel.CurrentTurn);
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

		private void OnPlayerBankedPointsChangedHandler(int oldValue, int newValue)
		{
			diceTableView.SetPlayerBankedPointsText(oldValue, newValue);
		}
		
		private void OnEnemyBankedPointsChangedHandler(int oldValue, int newValue)
		{
			diceTableView.SetEnemyBankedPointsText(oldValue, newValue);
		}

		private void OnTargetPointsChangedHandler(int oldValue, int newValue)
		{
			diceTableView.SetTargetPointsText(oldValue, newValue);
		}

		private void OnCurrentTurnChangedHandler(int oldValue, int newValue)
		{
			diceTableView.SwitchTurn(diceGameModel.IsPlayerTurn);
			diceTableView.SetTurnText(oldValue, newValue);
		}

		private void OnTurnPointsChangedHandler(int oldValue, int newValue)
		{
			diceTableView.SetCurrentPointsText(oldValue, newValue);
		}
		
		private void OnPreviewPointsChangedHandler(int oldValue, int newValue)
		{
			diceTableView.SetPreviewPointsText(oldValue, newValue);
		}
	}
}
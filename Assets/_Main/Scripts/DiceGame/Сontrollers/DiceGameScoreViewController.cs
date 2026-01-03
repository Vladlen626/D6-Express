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
			tableModel.OnBankedPointsChanged += OnBankedPointsChangedHandler;
			tableModel.OnTurnPointsChanged += OnTurnPointsChangedHandler;
			tableModel.OnPreviewPointsChanged += OnPreviewPointsChangedHandler;

			OnBankedPointsChangedHandler();
			OnTargetPointsChangedHandler();
			OnTurnPointsChangedHandler();
			OnPreviewPointsChangedHandler();
			OnCurrentTurnChangedHandler();
		}

		public void Deactivate()
		{
			diceGameModel.OnTargetPointsChanged -= OnTargetPointsChangedHandler;
			diceGameModel.OnCurrentTurnChanged -= OnCurrentTurnChangedHandler;
			tableModel.OnBankedPointsChanged -= OnBankedPointsChangedHandler;
			tableModel.OnTurnPointsChanged -= OnTurnPointsChangedHandler;
			tableModel.OnPreviewPointsChanged -= OnPreviewPointsChangedHandler;
		}

		private void OnBankedPointsChangedHandler()
		{
			diceTableView.SetBankedPointsText(tableModel.BankedPoints.ToString());
		}

		private void OnTargetPointsChangedHandler()
		{
			diceTableView.SetTargetPointsText(diceGameModel.TargetPoints.ToString());
		}

		private void OnCurrentTurnChangedHandler()
		{
			diceTableView.SetTurnText(diceGameModel.CurrentTurn, diceGameModel.MaxTurnCount);
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
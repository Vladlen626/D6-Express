using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

namespace _Main.Scripts.Dice
{
	public class DiceGameScoreController : IBaseController, IActivatable
	{
		private readonly DiceGameModel diceGameModel;
		private readonly TurnModel turnModel;
		private readonly DiceTableView diceTableView;

		public DiceGameScoreController(DiceGameModel inDiceGameModel, TurnModel inTurnModel, DiceTableView inDiceTableView)
		{
			diceGameModel = inDiceGameModel;
			turnModel = inTurnModel;
			diceTableView = inDiceTableView;
		}
		public void Activate()
		{
			diceGameModel.OnBankedPointsChanged += OnBankedPointsChangedHandler;
			diceGameModel.OnTargetPointsChanged += OnTargetPointsChangedHandler;
			turnModel.OnTurnPointsChanged += OnTurnPointsChangedHandler;
			turnModel.OnPreviewPointsChanged += OnPreviewPointsChangedHandler;
			
			
			OnBankedPointsChangedHandler();
			OnTargetPointsChangedHandler();
			OnTurnPointsChangedHandler();
			OnPreviewPointsChangedHandler();
		}

		public void Deactivate()
		{
			diceGameModel.OnBankedPointsChanged -= OnBankedPointsChangedHandler;
			diceGameModel.OnTargetPointsChanged -= OnTargetPointsChangedHandler;
			turnModel.OnTurnPointsChanged -= OnTurnPointsChangedHandler;
			turnModel.OnPreviewPointsChanged -= OnPreviewPointsChangedHandler;
		}

		private void OnBankedPointsChangedHandler()
		{
			diceTableView.SetBankedPointsText(diceGameModel.BankedPoints.ToString());
		}

		private void OnTargetPointsChangedHandler()
		{
			diceTableView.SetTargetPointsText(diceGameModel.TargetPoints.ToString());
		}

		private void OnTurnPointsChangedHandler()
		{
			diceTableView.SetCurrentPointsText(turnModel.TurnPoints.ToString());
		}
		
		private void OnPreviewPointsChangedHandler()
		{
			diceTableView.SetPreviewPointsText(turnModel.PreviewPoints.ToString());
		}
	}
}
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
			turnModel.OnTurnPointsChanged += OnCurrentScoreChangedHandler;
			
			
			OnBankedPointsChangedHandler();
			OnTargetPointsChangedHandler();
			OnCurrentScoreChangedHandler();
		}

		public void Deactivate()
		{
			diceGameModel.OnBankedPointsChanged -= OnBankedPointsChangedHandler;
			diceGameModel.OnTargetPointsChanged -= OnTargetPointsChangedHandler;
			turnModel.OnTurnPointsChanged -= OnCurrentScoreChangedHandler;
		}

		private void OnBankedPointsChangedHandler()
		{
			diceTableView.SetBankedScoreText(diceGameModel.BankedPoints.ToString());
		}

		private void OnTargetPointsChangedHandler()
		{
			diceTableView.SetTargetScoreText(diceGameModel.TargetPoints.ToString());
		}

		private void OnCurrentScoreChangedHandler()
		{
			diceTableView.SetCurrentScoreText(turnModel.TurnPoints.ToString());
		}
	}
}
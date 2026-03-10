using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;

namespace _Main.Scripts.Dice
{
	public class DiceGameViewController : IBaseController, IActivatable
	{
		private readonly DiceGameModel diceGameModel;
		private readonly DiceTableView diceTableView;
		private readonly ICameraShakeService cameraShakeService;
		private readonly GlobalNotificationService notificationService;
		private TableModel tableModel => diceGameModel.tableModel;

		public DiceGameViewController(
			DiceTableView diceTableView,
			DiceGameModel diceGameModel,
			ICameraShakeService cameraShakeService,
			GlobalNotificationService notificationService)
		{
			this.diceTableView = diceTableView;
			this.diceGameModel = diceGameModel;
			this.cameraShakeService = cameraShakeService;
			this.notificationService = notificationService;
		}
		public void Activate()
		{
			diceGameModel.OnTargetPointsChanged += OnTargetPointsChangedHandler;
			diceGameModel.OnCurrentTurnChanged += OnCurrentTurnChangedHandler;

			diceTableView.OnPassClicked += diceGameModel.SendPassClicked;
			diceTableView.OnRollClicked += diceGameModel.SendRollClicked;

			tableModel.OnPlayerBankedPointsChanged += OnPlayerBankedPointsChangedHandler;
			tableModel.OnEnemyBankedPointsChanged += OnEnemyBankedPointsChangedHandler;
			tableModel.OnTurnPointsChanged += OnTurnPointsChangedHandler;
			tableModel.OnPreviewPointsChanged += OnPreviewPointsChangedHandler;
			tableModel.OnUpdateUI += UpdateUI;
			tableModel.OnDisableButtons += DisableButtons;

			OnPlayerBankedPointsChangedHandler(0, tableModel.PlayerBankedPoints);
			OnTargetPointsChangedHandler(0, diceGameModel.TargetPoints);
			OnTurnPointsChangedHandler(0, tableModel.TurnPoints);
			OnPreviewPointsChangedHandler(0, tableModel.PreviewPoints);
			OnCurrentTurnChangedHandler(0, diceGameModel.CurrentTurn);

			UpdateUI();
		}

		public void Deactivate()
		{
			diceGameModel.OnTargetPointsChanged -= OnTargetPointsChangedHandler;
			diceGameModel.OnCurrentTurnChanged -= OnCurrentTurnChangedHandler;

			diceTableView.OnPassClicked -= diceGameModel.SendPassClicked;
			diceTableView.OnRollClicked -= diceGameModel.SendRollClicked;

			tableModel.OnPlayerBankedPointsChanged -= OnPlayerBankedPointsChangedHandler;
			tableModel.OnEnemyBankedPointsChanged -= OnEnemyBankedPointsChangedHandler;
			tableModel.OnTurnPointsChanged -= OnTurnPointsChangedHandler;
			tableModel.OnPreviewPointsChanged -= OnPreviewPointsChangedHandler;
			tableModel.OnUpdateUI -= UpdateUI;
			tableModel.OnDisableButtons -= DisableButtons;
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
			if (diceGameModel.DiceGameState == DiceGameState.GAME && notificationService != null)
			{
				var id = diceGameModel.IsPlayerTurn ? "dice_banner_turn_player" : "dice_banner_turn_enemy";
				notificationService.ShowBanner(id, 0.9f, !diceGameModel.IsPlayerTurn);
			}
		}

		private void OnTurnPointsChangedHandler(int oldValue, int newValue)
		{
			diceTableView.SetCurrentPointsText(oldValue, newValue);
		}
		
		private void OnPreviewPointsChangedHandler(int oldValue, int newValue)
		{
			diceTableView.SetPreviewPointsText(oldValue, newValue);
		}

		private void OnRollClickedHandler()
		{
			cameraShakeService.ShakeAsync(0.3f, 0.05f);
		}

		private void OnRollHandler()
		{
			UpdateUI();
		}

		private void OnPassHandler()
		{
			UpdateUI();
		}

		public void UpdateUI()
		{
			bool hasValidComboSelected = tableModel.PreviewPoints > 0;
			bool canPass = hasValidComboSelected;
			bool canRoll = tableModel.isFirstRoll || hasValidComboSelected;

			diceTableView.SetButtonInteractable("Roll", canRoll && diceGameModel.IsPlayerTurn);
			diceTableView.SetButtonInteractable("Pass", canPass && diceGameModel.IsPlayerTurn);
		}

		public void DisableButtons()
		{
			diceTableView.SetButtonInteractable("Roll", false);
			diceTableView.SetButtonInteractable("Pass", false);
		}
	}
}

using _Main.Scripts.Core;
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
		private readonly ILocalizationService localizationService;
		private TableModel tableModel => diceGameModel.tableModel;
		private bool isMatchResolved;
		private string hintProcessSelectText;
		private string hintProcessInvalidText;

		public DiceGameViewController(
			DiceTableView diceTableView,
			DiceGameModel diceGameModel,
			ICameraShakeService cameraShakeService,
			GlobalNotificationService notificationService,
			ILocalizationService localizationService)
		{
			this.diceTableView = diceTableView;
			this.diceGameModel = diceGameModel;
			this.cameraShakeService = cameraShakeService;
			this.notificationService = notificationService;
			this.localizationService = localizationService;
		}

		public void Activate()
		{
			isMatchResolved = false;
			hintProcessSelectText = localizationService.GetLocalized(GlobalConstants.Localization.DiceHintProcessSelect);
			hintProcessInvalidText = localizationService.GetLocalized(GlobalConstants.Localization.DiceHintProcessInvalid);

			diceGameModel.OnTargetPointsChanged += OnTargetPointsChangedHandler;
			diceGameModel.OnCurrentTurnChanged += OnCurrentTurnChangedHandler;
			diceGameModel.OnItemTargetingChanged += OnItemTargetingChangedHandler;
			diceGameModel.OnGameConditionPassed += OnGameConditionPassedHandler;
			diceGameModel.OnGameConditionFailed += OnGameConditionFailedHandler;

			diceTableView.OnPassClicked += diceGameModel.SendPassClicked;
			diceTableView.OnRollClicked += diceGameModel.SendRollClicked;
			diceTableView.OnRollClicked += OnRollClickedHandler;

			tableModel.OnPlayerBankedPointsChanged += OnPlayerBankedPointsChangedHandler;
			tableModel.OnEnemyBankedPointsChanged += OnEnemyBankedPointsChangedHandler;
			tableModel.OnTurnPointsChanged += OnTurnPointsChangedHandler;
			tableModel.OnPreviewPointsChanged += OnPreviewPointsChangedHandler;
			tableModel.OnUpdateUI += UpdateUI;
			tableModel.OnDisableButtons += DisableButtons;

			OnPlayerBankedPointsChangedHandler(0, tableModel.PlayerBankedPoints);
			OnEnemyBankedPointsChangedHandler(0, tableModel.EnemyBankedPoints);
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
			diceGameModel.OnItemTargetingChanged -= OnItemTargetingChangedHandler;
			diceGameModel.OnGameConditionPassed -= OnGameConditionPassedHandler;
			diceGameModel.OnGameConditionFailed -= OnGameConditionFailedHandler;

			diceTableView.OnPassClicked -= diceGameModel.SendPassClicked;
			diceTableView.OnRollClicked -= diceGameModel.SendRollClicked;
			diceTableView.OnRollClicked -= OnRollClickedHandler;

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

		private void OnItemTargetingChangedHandler(bool oldValue, bool newValue)
		{
			UpdateUI();
		}

		private void OnPreviewPointsChangedHandler(int oldValue, int newValue)
		{
			diceTableView.SetPreviewPointsText(oldValue, newValue);
		}

		private void OnGameConditionPassedHandler()
		{
			isMatchResolved = true;
			DisableButtons();
		}

		private void OnGameConditionFailedHandler()
		{
			isMatchResolved = true;
			DisableButtons();
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
			bool isTargetingActive = diceGameModel.IsItemTargetingActive;
			bool canPass = hasValidComboSelected && !isTargetingActive;
			bool canRoll = (tableModel.isFirstRoll || hasValidComboSelected) && !isTargetingActive;
			bool canInteract = !isMatchResolved && diceGameModel.IsPlayerActionPhase;

			diceTableView.SetButtonInteractable("Roll", canRoll && canInteract);
			diceTableView.SetButtonInteractable("Pass", canPass && canInteract);
			UpdateDiceProcessHint(hasValidComboSelected, isTargetingActive, canInteract);
		}

		public void DisableButtons()
		{
			diceTableView.SetButtonInteractable("Roll", false);
			diceTableView.SetButtonInteractable("Pass", false);
			diceTableView.SetDiceProcessHintText(string.Empty);
		}

		private void UpdateDiceProcessHint(bool hasValidComboSelected, bool isTargetingActive, bool canInteract)
		{
			if (diceGameModel.DiceGameState != DiceGameState.GAME ||
				!diceGameModel.IsPlayerTurn ||
				tableModel.isFirstRoll ||
				isTargetingActive ||
				!canInteract)
			{
				diceTableView.SetDiceProcessHintText(string.Empty);
				return;
			}

			var selectedDiceCount = diceGameModel.GetSelected().Length;
			if (selectedDiceCount <= 0)
			{
				diceTableView.SetDiceProcessHintText(hintProcessSelectText);
				return;
			}

			if (hasValidComboSelected)
			{
				diceTableView.SetDiceProcessHintText(string.Empty);
				return;
			}

			diceTableView.SetDiceProcessHintText(hintProcessInvalidText);
		}
	}
}

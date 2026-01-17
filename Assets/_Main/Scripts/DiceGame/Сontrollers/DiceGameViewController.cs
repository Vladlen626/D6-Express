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
		private TableModel tableModel => diceGameModel.tableModel;

		public DiceGameViewController(
			DiceTableView diceTableView,
			DiceGameModel diceGameModel,
			ICameraShakeService cameraShakeService)
		{
			this.diceTableView = diceTableView;
			this.diceGameModel = diceGameModel;
			this.cameraShakeService = cameraShakeService;
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
			tableModel.OnUpdateUI += UpdateUIHandler;
			tableModel.OnDisableButtons += DisableButtons;
			
			foreach (var diceModel in diceGameModel.CurrentDiceModelList)
			{
				diceModel.OnDiceChosenChanged += UpdateUIHandler;
			}

			OnPlayerBankedPointsChangedHandler(0, tableModel.PlayerBankedPoints);
			OnTargetPointsChangedHandler(0, diceGameModel.TargetPoints);
			OnTurnPointsChangedHandler(0, tableModel.TurnPoints);
			OnPreviewPointsChangedHandler(0, tableModel.PreviewPoints);
			OnCurrentTurnChangedHandler(0, diceGameModel.CurrentTurn);

			UpdateUIHandler();
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
			tableModel.OnUpdateUI -= UpdateUIHandler;
			tableModel.OnDisableButtons -= DisableButtons;
			
			foreach (var diceModel in diceGameModel.CurrentDiceModelList)
			{
				diceModel.OnDiceChosenChanged -= UpdateUIHandler;
			}
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

		private void OnRollClickedHandler()
		{
			cameraShakeService.ShakeAsync(diceTableView.TableCamera,0.3f, 0.05f);
		}

		private void OnRollHandler()
		{
			UpdateUIHandler();
		}

		private void OnPassHandler()
		{
			UpdateUIHandler();
		}

		public void UpdateUIHandler()
		{
			var selectedDice = diceGameModel.GetSelected();
			var selectedValues = new int[selectedDice.Length];
			for (int i = 0; i < selectedDice.Length; i++)
			{
				selectedValues[i] = selectedDice[i].CurrentValue;
			}

			int scorePreview = DiceGameUtils.CalculateScore(selectedValues);
			bool hasValidComboSelected = scorePreview > 0;
			bool canPass = hasValidComboSelected || (tableModel.TurnPoints > 0 && selectedDice.Length == 0);
			bool canRoll = tableModel.isFirstRoll || hasValidComboSelected;

			int previewPoints = hasValidComboSelected ? scorePreview : 0;
			tableModel.SetPreviewPoints(previewPoints);

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
using System;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;

namespace _Main.Scripts.Dice
{
	public class DiceGameProcessController : IBaseController, IActivatable
	{
		private readonly ILoggerService logger;
		private readonly DiceGameModel diceGameModel;
		private readonly TableModel tableModel;

		private readonly DiceTableView tableView;
		private readonly DicePoolLogic dicePool;


		public DiceGameProcessController(
			TableModel tableModel,
			DiceTableView tableView,
			ILoggerService logger,
			DiceGameModel  diceGameModel)
		{
			this.tableModel = tableModel;
			this.tableView = tableView;
			this.logger = logger;
			this.diceGameModel = diceGameModel;

			dicePool = new DicePoolLogic(diceGameModel);
		}

		public void Activate()
		{
			logger?.Log("[DiceGameController] Activating...");

			tableView.OnRollClicked += HandleRoll;
			tableView.OnPassClicked += HandlePass;

			foreach (var diceModel in diceGameModel.GameSelectedDiceModelsList)
			{
				diceModel.OnDiceChosenChanged += UpdateUI;
			}
			
			UpdateUI();
		}

		public void Deactivate()
		{
			logger?.Log("[DiceGameController] Deactivating...");

			tableView.OnRollClicked -= HandleRoll;
			tableView.OnPassClicked -= HandlePass;

			foreach (var diceModel in diceGameModel.GameSelectedDiceModelsList)
			{
				diceModel.OnDiceChosenChanged -= UpdateUI;
			}
		}

		// === ОБРАБОТЧИКИ КНОПОК ===

		private void HandleRoll()
		{
			logger?.Log("[DiceGameController] Handle roll");

			if (tableModel.isFirstRoll)
			{
				tableModel.isFirstRoll = false;
				diceGameModel.ShowAllDiceGameModels();
			}

			// Сохраняем выбранные кубы, если есть
			bool isHotDice = TrySaveSelected();
			tableModel.SetPreviewPoints(0);

			// Если все кубы забанкированы после сохранения, сбросить пул
			if (isHotDice)
			{
				tableModel.ResetAllPositions();
				dicePool.ResetAll();
			}

			// Роллим актуальные кубы
			var diceToRoll = dicePool.GetUnbanked();
			foreach (var dice in diceToRoll)
			{
				dice.Roll();
			}
			
			CheckBust();
			UpdateUI();
		}

		private void CheckBust()
		{
			var diceToRoll = dicePool.GetUnbanked();
			if (DiceGameUtils.RollHasAnyScore(GetValues(diceToRoll)))
			{
				return;
			}

			logger?.Log("[DiceGameController] BUST!");
			EndTurn(false);
		}

		private int[] GetValues(DiceModel[] dice)
		{
			var values = new int[dice.Length];
			for (int i = 0; i < dice.Length; i++) values[i] = dice[i].CurrentValue;
			return values;
		}

		private void HandlePass()
		{
			logger?.Log("[DiceGameController] Handle pass");

			TrySaveSelected();
			EndTurn(true);
		}

		private void EndTurn(bool success)
		{
			if (success)
			{
				tableModel.AddBankedPoints(tableModel.TurnPoints);
			}

			diceGameModel.IncreaseCurrentTurn();
			tableModel.ResetTurn();
			dicePool.ResetAll();
			UpdateUI();
		}

		private bool TrySaveSelected()
		{
			var selected = dicePool.GetSelected();
			if (selected.Length == 0)
			{
				return false;
			}

			var values = new int[selected.Length];
			for (var i = 0; i < selected.Length; i++)
			{
				values[i] = selected[i].CurrentValue;
			}

			int points = DiceGameUtils.CalculateScore(values);
			if (points <= 0)
			{
				return false;
			}

			tableModel.AddTurnPoints(points);
			dicePool.BankSelected();

			return dicePool.AllBanked();
		}

		private void UpdateUI()
		{
			if (tableModel.isFirstRoll)
			{
				diceGameModel.HideAllDiceGameModels();
			}
			else
			{
				diceGameModel.ShowAllDiceGameModels();
			}

			var selectedDice = dicePool.GetSelected();
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

			tableView.SetButtonInteractable("Roll", canRoll);
			tableView.SetButtonInteractable("Pass", canPass);
		}
	}
}
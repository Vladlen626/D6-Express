using System;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;

namespace _Main.Scripts.Dice
{
	public class DiceGameProcessController : IBaseController, IActivatable
	{
		private readonly TableModel tableModel;

		private readonly DiceModel[] diceModels;
		private readonly DiceTableView tableView;

		private readonly DicePoolLogic dicePool;
		private readonly ILoggerService logger;

		public DiceGameProcessController(
			TableModel tableModel,
			DiceModel[] diceModels,
			DiceTableView tableView,
			ILoggerService logger)
		{
			this.tableModel = tableModel;
			this.diceModels = diceModels;
			this.tableView = tableView;
			this.logger = logger;

			dicePool = new DicePoolLogic(diceModels);
		}

		public void Activate()
		{
			logger?.Log("[DiceGameController] Activating...");

			tableView.OnRollClicked += HandleRoll;
			tableView.OnPassClicked += HandlePass;

			foreach (var diceModel in diceModels)
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

			foreach (var diceModel in diceModels)
			{
				diceModel.OnDiceChosenChanged -= UpdateUI;
			}
		}

		// === ОБРАБОТЧИКИ КНОПОК ===

		private void HandleRoll()
		{
			logger?.Log("[DiceGameController] 🎲 ReRoll button pressed");

			// 1. Сохраняем выбранные кубы
			var selectedDice = dicePool.GetSelected();
			if (selectedDice.Length > 0)
			{
				var selectedValues = new int[selectedDice.Length];
				for (var i = 0; i < selectedDice.Length; i++)
				{
					selectedValues[i] = selectedDice[i].CurrentValue;
				}

				// ИЗМЕНЕНО: проверка через CalculateScore
				int points = DiceGameUtils.CalculateScore(selectedValues);
				if (points < 0)
				{
					logger?.LogWarning("[DiceGameController] Invalid selection!");
					return;
				}

				tableModel.AddTurnPoints(points);
				logger?.Log($"[DiceGameController] Scored {points} points. Turn total: {tableModel.TurnPoints}");

				dicePool.BankSelected();
			}

			// 2. Очищаем превью
			tableModel.SetPreviewPoints(0);

			// 3. Проверка на HOT DICE (все кубы забанкованы)
			if (dicePool.AllBanked())
			{
				logger?.Log("[DiceGameController] 🔥 HOT DICE! Resetting all dice.");
				tableModel.ResetAllPositions();
				dicePool.ResetAll();
			}

			// 4. Бросаем оставшиеся кубы
			var unbankedDice = dicePool.GetUnbanked();
			foreach (var dice in unbankedDice)
			{
				dice.Roll();
			}

			// 5. Проверка на BUST
			var rolledValues = new int[unbankedDice.Length];
			for (var i = 0; i < unbankedDice.Length; i++)
			{
				rolledValues[i] = unbankedDice[i].CurrentValue;
			}

			// ИЗМЕНЕНО: используем RollHasAnyScore вместо IsBust
			if (!DiceGameUtils.RollHasAnyScore(rolledValues))
			{
				logger?.Log("[DiceGameController] ❌ BUST! Turn points lost.");
				tableModel.ResetTurn();
				HandlePass();
				return;
			}

			UpdateUI();
		}

		private void HandlePass()
		{
			logger?.Log("[DiceGameController] ✋ Pass button pressed");

			// 1. Сохраняем выбранные кубы (если есть)
			var selectedDice = dicePool.GetSelected();
			if (selectedDice.Length > 0)
			{
				var selectedValues = new int[selectedDice.Length];
				for (var i = 0; i < selectedDice.Length; i++)
				{
					selectedValues[i] = selectedDice[i].CurrentValue;
				}

				// ИЗМЕНЕНО: проверка через CalculateScore
				int points = DiceGameUtils.CalculateScore(selectedValues);
				if (points > 0)
				{
					tableModel.AddTurnPoints(points);
					logger?.Log($"[DiceGameController] Scored {points} points. Turn total: {tableModel.TurnPoints}");
				}
			}

			// 2. Банкуем очки хода в общий счет
			tableModel.AddBankedPoints(tableModel.TurnPoints);
			logger?.Log(
				$"[DiceGameController] Banked {tableModel.TurnPoints} points. Total banked: {tableModel.BankedPoints}");

			// 3. Сбрасываем ход
			tableModel.ResetTurn();
			dicePool.ResetAll();

			// 4. Бросаем все кубы для начала нового хода
			foreach (var dice in diceModels)
			{
				dice.Roll();
			}

			UpdateUI();
		}

		private void UpdateUI()
		{
			var selectedDice = dicePool.GetSelected();
			var selectedValues = new int[selectedDice.Length];
			for (int i = 0; i < selectedDice.Length; i++)
			{
				selectedValues[i] = selectedDice[i].CurrentValue;
			}

			int scorePreview = DiceGameUtils.CalculateScore(selectedValues);
			bool hasValidComboSelected = scorePreview > 0;
			bool canPass = hasValidComboSelected || (tableModel.TurnPoints > 0 && selectedDice.Length == 0);

			int previewPoints = hasValidComboSelected ? scorePreview : 0;
			tableModel.SetPreviewPoints(previewPoints);

			tableView.SetButtonInteractable("Roll", hasValidComboSelected);
			tableView.SetButtonInteractable("Pass", canPass);
		}
	}
}
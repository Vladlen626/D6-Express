using System;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;

namespace _Main.Scripts.Dice
{
	public class DiceGameController : IBaseController, IActivatable
	{
		private readonly DiceGameModel _diceGameModel;
		private readonly TurnModel _turnModel;

		private readonly DiceModel[] _diceModels;
		private readonly DiceTableView _tableView;

		private readonly DicePoolLogic _dicePool;
		private readonly ILoggerService _logger;

		public DiceGameController(
			DiceGameModel diceGameModel,
			TurnModel turnModel,
			DiceModel[] diceModels,
			DiceTableView tableView,
			ILoggerService logger)
		{
			_diceGameModel = diceGameModel;
			_turnModel = turnModel;
			_diceModels = diceModels;
			_tableView = tableView;
			_logger = logger;

			_dicePool = new DicePoolLogic(diceModels);
		}

		public void Activate()
		{
			_logger?.Log("[DiceGameController] Activating...");

			_tableView.OnRollClicked += HandleRoll;
			_tableView.OnPassClicked += HandlePass;

			foreach (var diceModel in _diceModels)
			{
				diceModel.OnDiceChosenChanged += UpdateUI;
			}

			UpdateUI();
		}

		public void Deactivate()
		{
			_logger?.Log("[DiceGameController] Deactivating...");

			_tableView.OnRollClicked -= HandleRoll;
			_tableView.OnPassClicked -= HandlePass;

			foreach (var diceModel in _diceModels)
			{
				diceModel.OnDiceChosenChanged -= UpdateUI;
			}
		}

		// === ОБРАБОТЧИКИ КНОПОК ===

		private void HandleRoll()
		{
			_logger?.Log("[DiceGameController] 🎲 ReRoll button pressed");

			// 1. Сохраняем выбранные кубы
			var selectedDice = _dicePool.GetSelected();
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
					_logger?.LogWarning("[DiceGameController] Invalid selection!");
					return;
				}

				_turnModel.AddTurnPoints(points);
				_logger?.Log($"[DiceGameController] Scored {points} points. Turn total: {_turnModel.TurnPoints}");

				_dicePool.BankSelected();
			}

			// 2. Очищаем превью
			_turnModel.SetPreviewPoints(0);

			// 3. Проверка на HOT DICE (все кубы забанкованы)
			if (_dicePool.AllBanked())
			{
				_logger?.Log("[DiceGameController] 🔥 HOT DICE! Resetting all dice.");
				_diceGameModel.ResetAllPositions();
				_dicePool.ResetAll();
			}

			// 4. Бросаем оставшиеся кубы
			var unbankedDice = _dicePool.GetUnbanked();
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
				_logger?.Log("[DiceGameController] ❌ BUST! Turn points lost.");
				_turnModel.Reset();
				HandlePass();
				return;
			}

			UpdateUI();
		}

		private void HandlePass()
		{
			_logger?.Log("[DiceGameController] ✋ Pass button pressed");

			// 1. Сохраняем выбранные кубы (если есть)
			var selectedDice = _dicePool.GetSelected();
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
					_turnModel.AddTurnPoints(points);
					_logger?.Log($"[DiceGameController] Scored {points} points. Turn total: {_turnModel.TurnPoints}");
				}
			}

			// 2. Банкуем очки хода в общий счет
			_diceGameModel.AddBankedPoints(_turnModel.TurnPoints);
			_logger?.Log(
				$"[DiceGameController] Banked {_turnModel.TurnPoints} points. Total banked: {_diceGameModel.BankedPoints}");

			// 3. Сбрасываем ход
			_turnModel.Reset();
			_diceGameModel.ResetAllPositions();
			_dicePool.ResetAll();

			// 4. Бросаем все кубы для начала нового хода
			foreach (var dice in _diceModels)
			{
				dice.Roll();
			}

			UpdateUI();
		}

		private void UpdateUI()
		{
			var selectedDice = _dicePool.GetSelected();
			var selectedValues = new int[selectedDice.Length];
			for (int i = 0; i < selectedDice.Length; i++)
			{
				selectedValues[i] = selectedDice[i].CurrentValue;
			}

			int scorePreview = DiceGameUtils.CalculateScore(selectedValues);
			bool hasValidComboSelected = scorePreview > 0;
			bool canPass = hasValidComboSelected || (_turnModel.TurnPoints > 0 && selectedDice.Length == 0);

			int previewPoints = hasValidComboSelected ? scorePreview : 0;
			_turnModel.SetPreviewPoints(previewPoints);

			_tableView.SetButtonInteractable("Roll", hasValidComboSelected);
			_tableView.SetButtonInteractable("Pass", canPass);
		}
	}
}
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
			_tableView.OnSaveClicked += HandleSave;
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
			_tableView.OnSaveClicked -= HandleSave;
			_tableView.OnPassClicked -= HandlePass;

			foreach (var diceModel in _diceModels)
			{
				diceModel.OnDiceChosenChanged -= UpdateUI;
			}
		}

		// === ОБРАБОТЧИКИ КНОПОК ===

		private void HandleRoll()
		{
			_logger?.Log("[DiceGameController] 🎲 Roll button pressed");

			var selectedDice = _dicePool.GetSelected();
			if (selectedDice.Length > 0)
			{
				HandleSave();
			}

			var unbankedDice = _dicePool.GetUnbanked();
			foreach (var dice in unbankedDice)
			{
				dice.Roll();
			}

			var rolledValues = new int[unbankedDice.Length];
			for (var i = 0; i < unbankedDice.Length; i++)
			{
				rolledValues[i] = unbankedDice[i].CurrentValue;
			}

			if (DiceGameUtils.IsBust(rolledValues))
			{
				_logger?.Log("[DiceGameController] ❌ BUST! Turn points lost.");
				_turnModel.Reset();
				HandlePass();
			}

			UpdateUI();
		}

		private void HandleSave()
		{
			_logger?.Log("[DiceGameController] 💾 Save button pressed");

			var selectedDiceArray = _dicePool.GetSelected();
			if (selectedDiceArray.Length == 0)
			{
				_logger?.LogWarning("[DiceGameController] No dice selected!");
				return;
			}

			var selectedValues = new int[selectedDiceArray.Length];
			for (var i = 0; i < selectedDiceArray.Length; i++)
			{
				selectedValues[i] = selectedDiceArray[i].CurrentValue;
			}

			if (!DiceGameUtils.HasValidCombo(selectedValues))
			{
				_logger?.LogWarning("[DiceGameController] Invalid selection!");
				return;
			}

			int points = DiceGameUtils.CalculateScore(selectedValues);
			_turnModel.AddTurnPoints(points);

			string comboName = DiceGameUtils.GetComboName(selectedValues);
			_logger?.Log(
				$"[DiceGameController] Scored {points} points ({comboName}). Turn total: {_turnModel.TurnPoints}");

			_dicePool.BankSelected();

			if (_dicePool.AllBanked())
			{
				_logger?.Log("[DiceGameController] 🔥 HOT DICE! Resetting all dice.");
				ResetTable();
			}

			UpdateUI();
		}

		private void HandlePass()
		{
			_logger?.Log("[DiceGameController] ✋ Pass button pressed");

			_diceGameModel.AddBankedPoints(_turnModel.TurnPoints);
			_logger?.Log(
				$"[DiceGameController] Banked {_turnModel.TurnPoints} points. Total banked: {_diceGameModel.BankedPoints}");

			_turnModel.Reset();
			ResetTable();

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

			bool hasValidComboSelected = DiceGameUtils.HasValidCombo(selectedValues);
			
			bool canRoll = _dicePool.HasUnbanked() && selectedDice.Length == 0;
			bool canSave = hasValidComboSelected;
			bool canPass = _turnModel.TurnPoints > 0 && selectedDice.Length == 0;

			_tableView.SetButtonInteractable("Roll", canRoll);
			_tableView.SetButtonInteractable("Save", canSave);
			_tableView.SetButtonInteractable("Pass", canPass);
		}

		private void ResetTable()
		{
			_diceGameModel.ResetAllPositions();
			_dicePool.ResetAll();
		}
	}
}
using System;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;

namespace _Main.Scripts.Dice
{
	public class DiceGameController : IBaseController, IActivatable, IDeactivatable
	{
		public event Action<int> OnTurnPointsChanged;
		public event Action<int> OnBankedPointsChanged;
		public event Action OnBust;
		public event Action OnHotDice;
		public event Action<bool> OnGameEnded; // true = win, false = lose

		private readonly GameModel _gameModel;
		private readonly TurnModel _turnModel;

		private readonly DiceModel[] _diceModels; // 6 костей
		private readonly DiceTableView _tableView;

		private readonly DicePoolLogic _dicePool;
		private readonly ILoggerService _logger;

		public DiceGameController(
			GameModel gameModel,
			TurnModel turnModel,
			DiceModel[] diceModels,
			DiceTableView tableView,
			ILoggerService logger)
		{
			_gameModel = gameModel;
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

			UpdateUI();
		}

		public void Deactivate()
		{
			_logger?.Log("[DiceGameController] Deactivating...");

			_tableView.OnRollClicked -= HandleRoll;
			_tableView.OnSaveClicked -= HandleSave;
			_tableView.OnPassClicked -= HandlePass;
		}

		// === ОБРАБОТЧИКИ КНОПОК ===

		private void HandleRoll()
		{
			_logger?.Log("[DiceGameController] 🎲 Roll button pressed");

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
				OnBust?.Invoke();
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
				OnHotDice?.Invoke();
				_dicePool.ResetAll();
				HandleRoll();
			}

			OnTurnPointsChanged?.Invoke(_turnModel.TurnPoints);
			UpdateUI();
		}

		private void HandlePass()
		{
			_logger?.Log("[DiceGameController] ✋ Pass button pressed");

			_gameModel.AddBankedPoints(_turnModel.TurnPoints);
			_logger?.Log(
				$"[DiceGameController] Banked {_turnModel.TurnPoints} points. Total banked: {_gameModel.BankedPoints}");

			_turnModel.Reset();
			_dicePool.ResetAll();

			OnBankedPointsChanged?.Invoke(_gameModel.BankedPoints);

			if (_gameModel.BankedPoints >= GameModel.TARGET_SCORE)
			{
				_logger?.Log(
					$"[DiceGameController] 🎉 WIN! Reached {_gameModel.BankedPoints}/{GameModel.TARGET_SCORE}");
				OnGameEnded?.Invoke(true);
			}

			UpdateUI();
		}

		private void UpdateUI()
		{
			bool canRoll = _dicePool.HasUnbanked();
			bool canSave = _dicePool.GetSelected().Length > 0;
			bool canPass = _turnModel.TurnPoints > 0;

			_tableView.SetButtonInteractable("Roll", canRoll);
			_tableView.SetButtonInteractable("Save", canSave);
			_tableView.SetButtonInteractable("Pass", canPass);
		}
	}
}
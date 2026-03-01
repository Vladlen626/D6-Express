using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class DiceGameModel
	{
		public event Action<bool> OnEndTurn;
		public event Action ScreenDiceDictChanged;
		public event Action OnGameConditionPassed;
		public event Action OnGameConditionFailed;
		public event Action OnBetSizeChanged;
		public event Action<int, int> OnTargetPointsChanged;
		public event Action<int, int> OnCurrentTurnChanged;
		public event Action<int, int> OnMaxDiceCountChanged;
		public event Action OnDiceGameStateChanged;
		public event Action OnRollClicked;
		public event Action OnPassClicked;
		
		public TableModel tableModel;
		public ModifiersModel PlayerModifiersModel { get; }
		public ModifiersModel EnemyModifiersModel { get; }
		public ModifierItemsModel PlayerModifierItemsModel { get; }
		public ModifierItemsModel EnemyModifierItemsModel { get; }
		public DiceScoringService PlayerScoringService { get; }
		public DiceScoringService EnemyScoringService { get; }

		public ModifiersModel ModifiersModel => GetCurrentModifiersModel();
		public ModifierItemsModel ModifierItemsModel => GetCurrentModifierItemsModel();
		
		public List<DiceModel> CurrentDiceModelList => IsPlayerTurn ? PlayerDiceModelList : EnemyDiceModelList;
		
		public readonly List<DiceModel> EnemyDiceModelList = new();
		public readonly List<DiceModel> PlayerDiceModelList = new();
		public readonly List<DiceModel> SelectionDiceModelList = new();
		public IReadOnlyDictionary<DiceModel, DiceView> ScreenDiceDict => screenDiceDict;
		public Dictionary<DiceModel, DiceView> screenDiceDict = new ();

		public DiceGameState DiceGameState { get; private set; } = DiceGameState.DEFAULT;
		public int BetSize { get; private set; }
		public int MaxBetSize { get; private set; }
		public int MinBetSize { get; private set; }
		public int CurrentTurn { get; private set; }
		public bool IsPlayerTurn { get; private set; }
		public int TargetPoints { get; private set; }
		public bool IsConditionPassed { get; private set; }
		public bool IsDiceGameStarted { get; private set; }
		public bool EnemyComboUpgradesEnabled { get; private set; } = true;
		public int MaxDiceCount => Mathf.Max(1, baseMaxDiceCount + GetDiceCapBonusSum(IsPlayerTurn));
		public int BaseMaxDiceCount => baseMaxDiceCount;

		private const int DefaultMaxDiceCount = 6;
		private int baseMaxDiceCount = DefaultMaxDiceCount;
		private readonly Dictionary<string, int> playerDiceCapBonuses = new();
		private readonly Dictionary<string, int> enemyDiceCapBonuses = new();

		public DiceGameModel(
			InventoryModel inventoryModel,
			DiceScoringService playerScoringService = null,
			DiceScoringService enemyScoringService = null)
		{
			PlayerModifiersModel = inventoryModel?.ModifiersModel ?? new ModifiersModel();
			PlayerModifierItemsModel = inventoryModel?.ModifierItemsModel ?? new ModifierItemsModel(PlayerModifiersModel);
			PlayerModifierItemsModel.BindGameModel(this);

			EnemyModifiersModel = new ModifiersModel();
			EnemyModifierItemsModel = new ModifierItemsModel(EnemyModifiersModel);
			EnemyModifierItemsModel.BindGameModel(this);

			PlayerScoringService = playerScoringService ?? new DiceScoringService();
			EnemyScoringService = enemyScoringService ?? new DiceScoringService();
		}
		
		public void Setup(DiceGameConfig diceGameConfig, int maxBetSize, TableModel tableModel)
		{
			this.tableModel = tableModel;
			SetMinBetSize(diceGameConfig.min_bet_size);
			SetMaxBetSize(maxBetSize);
			SetBetSize((diceGameConfig.min_bet_size + maxBetSize) / 2);
			SetTargetScore(diceGameConfig.target_score);
			EnemyComboUpgradesEnabled = diceGameConfig.enemy_combo_upgrades_enabled;
			SetCurrentTurn(1, true);
		}

		/// <summary>
		/// Sets the base dice cap (without bonuses) and notifies listeners if the effective cap changes.
		/// </summary>
		public void SetBaseMaxDiceCount(int value)
		{
			value = Mathf.Max(1, value);
			var old = MaxDiceCount;
			baseMaxDiceCount = value;
			NotifyMaxDiceChanged(old);
		}

		/// <summary>
		/// Adds or replaces a dice cap bonus identified by a unique source id (e.g., item id).
		/// This makes the mechanic reusable by other modifiers/items.
		/// </summary>
		public void SetDiceCapModifier(string sourceId, int bonus)
		{
			SetDiceCapModifier(sourceId, bonus, IsPlayerTurn);
		}

		public void SetDiceCapModifier(string sourceId, int bonus, bool isPlayerSide)
		{
			if (string.IsNullOrWhiteSpace(sourceId))
			{
				return;
			}

			bonus = Mathf.Max(0, bonus);
			var old = MaxDiceCount;
			GetDiceCapBonuses(isPlayerSide)[sourceId] = bonus;
			NotifyMaxDiceChanged(old);
		}

		public void RemoveDiceCapModifier(string sourceId)
		{
			RemoveDiceCapModifier(sourceId, IsPlayerTurn);
		}

		public void RemoveDiceCapModifier(string sourceId, bool isPlayerSide)
		{
			if (string.IsNullOrWhiteSpace(sourceId))
			{
				return;
			}

			var old = MaxDiceCount;
			if (GetDiceCapBonuses(isPlayerSide).Remove(sourceId))
			{
				NotifyMaxDiceChanged(old);
			}
		}

		public ModifiersModel GetCurrentModifiersModel()
		{
			return IsPlayerTurn ? PlayerModifiersModel : EnemyModifiersModel;
		}

		public ModifiersModel GetModifiersModel(bool isPlayerSide)
		{
			return isPlayerSide ? PlayerModifiersModel : EnemyModifiersModel;
		}

		public ModifierItemsModel GetCurrentModifierItemsModel()
		{
			return IsPlayerTurn ? PlayerModifierItemsModel : EnemyModifierItemsModel;
		}

		public ModifierItemsModel GetModifierItemsModel(bool isPlayerSide)
		{
			return isPlayerSide ? PlayerModifierItemsModel : EnemyModifierItemsModel;
		}

		public DiceScoringService GetCurrentScoringService()
		{
			return IsPlayerTurn ? PlayerScoringService : EnemyScoringService;
		}

		public DiceScoringService GetScoringService(bool isPlayerSide)
		{
			return isPlayerSide ? PlayerScoringService : EnemyScoringService;
		}

		private int GetDiceCapBonusSum(bool isPlayerSide)
		{
			var sum = 0;
			foreach (var bonus in GetDiceCapBonuses(isPlayerSide).Values)
			{
				sum += bonus;
			}
			return sum;
		}

		private Dictionary<string, int> GetDiceCapBonuses(bool isPlayerSide)
		{
			return isPlayerSide ? playerDiceCapBonuses : enemyDiceCapBonuses;
		}

		private void NotifyMaxDiceChanged(int previous)
		{
			var current = MaxDiceCount;
			if (previous != current)
			{
				OnMaxDiceCountChanged?.Invoke(previous, current);
			}
		}
		
		public void ChangeDiceGameState(DiceGameState diceGameState)
		{
			DiceGameState = diceGameState;
			if (diceGameState == DiceGameState.GAME)
			{
				IsDiceGameStarted = true;
			}
			OnDiceGameStateChanged?.Invoke();
		}

		public void HideAllDiceGameModels()
		{
			foreach (var diceModel in PlayerDiceModelList)
			{
				diceModel.SetHide(true);
			}
			
			foreach (var diceModel in EnemyDiceModelList)
			{
				diceModel.SetHide(true);
			}
		}
		
		public void ShowAllDiceGameModels()
		{
			foreach (var diceModel in PlayerDiceModelList)
			{
				diceModel.SetHide(!IsPlayerTurn);
			}

			foreach (var diceModel in EnemyDiceModelList)
			{
				diceModel.SetHide(IsPlayerTurn);
			}
		}

		public void SetBetSize(int size)
		{
			BetSize = size;
			OnBetSizeChanged?.Invoke();
		}

		public void SetMinBetSize(int size)
		{
			MinBetSize = size;
			OnBetSizeChanged?.Invoke();
		}

		public void SetMaxBetSize(int size)
		{
			MaxBetSize = size;
			OnBetSizeChanged?.Invoke();
		}


		public void SetTargetScore(int score)
		{
			var oldValue = TargetPoints;
			TargetPoints = score;
			OnTargetPointsChanged?.Invoke(oldValue, TargetPoints);
		}

		public void SetEnemyComboUpgradesEnabled(bool enabled)
		{
			EnemyComboUpgradesEnabled = enabled;
		}

		public void IncreaseCurrentTurn()
		{
			SetCurrentTurn(CurrentTurn + 1, !IsPlayerTurn);
		}

		public void SetCurrentTurn(int turn, bool isPlayerTurn)
		{
			var oldValue = CurrentTurn;
			CurrentTurn = turn;
			IsPlayerTurn = isPlayerTurn;
			OnCurrentTurnChanged?.Invoke(oldValue, CurrentTurn);
		}

		public void SetConditionPassed()
		{
			IsConditionPassed = true;
			OnGameConditionPassed?.Invoke();
		}

		public void SetConditionFailed()
		{
			IsConditionPassed = false;
			OnGameConditionFailed?.Invoke();
		}

		public void AddDiceOnScreen(DiceModel diceModel, DiceView diceView)
		{
			screenDiceDict.Add(diceModel, diceView);
			ScreenDiceDictChanged?.Invoke();
		}

		public void RemoveDiceOnScreen(DiceModel diceModel)
		{
			screenDiceDict.Remove(diceModel);
			ScreenDiceDictChanged?.Invoke();
		}

		public void SendRollClicked()
		{
			OnRollClicked?.Invoke();
		}

		public void SendPassClicked()
		{
			OnPassClicked?.Invoke();
		}

		public void RollEnded()
		{
			tableModel.SendUpdateUI();
		}

		public void PassEnded()
		{
			tableModel.SendUpdateUI();
		}

		public void EndTurn(bool success)
		{
			OnEndTurn?.Invoke(success);
			HideAllDiceGameModels();
			if (success)
			{
				if (IsPlayerTurn)
				{
					tableModel.AddBankedPointsForPlayer(tableModel.TurnPoints);
				}
				else
				{
					tableModel.AddBankedPointsForEnemy(tableModel.TurnPoints);
				}
			}

			IncreaseCurrentTurn();
			tableModel.ResetTurn();
			ResetAllDices();
			foreach (var diceModel in CurrentDiceModelList)
			{
				ScreenDiceDict[diceModel].MoveToPosition(tableModel.GetFreeActivePosition().position);
			}
		}

		private bool IsEffectivelySaved(DiceModel dice)
		{
			if (dice == null)
			{
				return true;
			}

			if (dice.IsSaved)
			{
				return true;
			}

			return DiceGameUtils.IsDiceBanked(dice, tableModel);
		}

		public DiceModel[] GetSelected()
		{
			return CurrentDiceModelList.Where(d => d.IsChosen && !IsEffectivelySaved(d)).ToArray();
		}

		public DiceModel[] GetUnbanked()
		{
			return CurrentDiceModelList.Where(d => !IsEffectivelySaved(d)).ToArray();
		}

		public DiceModel[] GetBanked()
		{
			return CurrentDiceModelList.Where(d => IsEffectivelySaved(d)).ToArray();
		}

		public bool HasUnbanked()
		{
			return CurrentDiceModelList.Any(d => !IsEffectivelySaved(d));
		}

		public bool AllBanked()
		{
			return CurrentDiceModelList.All(d => IsEffectivelySaved(d));
		}

		public void ResetAllDices()
		{
			foreach (var dice in CurrentDiceModelList)
			{
				dice.Reset();
			}
		}

		public void Reset()
		{
			tableModel.Reset();
			PlayerDiceModelList.Clear();
			EnemyDiceModelList.Clear();
			ResetEnemyRuntime();
			DiceGameState = DiceGameState.DEFAULT;
			IsDiceGameStarted = false;
			IsConditionPassed = false;
			CurrentTurn = 0;
		}

		private void ResetEnemyRuntime()
		{
			EnemyModifierItemsModel.Reset();
			EnemyModifiersModel.Reset();
			enemyDiceCapBonuses.Clear();
			EnemyScoringService.ResetUpgradeStatesToDefaults();
		}
	}
}

using System;
using System.Collections.Generic;

namespace _Main.Scripts.Dice
{
	public class DiceGameModel
	{
		public event Action ScreenDiceDictChanged;
		public event Action OnGameConditionPassed;
		public event Action OnGameConditionFailed;
		public event Action OnBetSizeChanged;
		public event Action OnTargetPointsChanged;
		public event Action OnCurrentTurnChanged;
		public event Action OnDiceGameStateChanged;

		public DiceGameState DiceGameState { get; private set; } = DiceGameState.DEFAULT;
		public int BetSize { get; private set; }
		public int MaxBetSize { get; private set; }
		public int MinBetSize { get; private set; }
		public int CurrentTurn { get; private set; }
		public bool IsPlayerTurn { get; private set; }
		public int TargetPoints { get; private set; }
		public bool IsConditionPassed { get; private set; }
		public bool IsDiceGameStarted { get; private set; }
		public List<DiceModel> CurrentDiceModelList => IsPlayerTurn ? PlayerDiceModelList : EnemyDiceModelList;
		public readonly List<DiceModel> EnemyDiceModelList = new();
		public readonly List<DiceModel> PlayerDiceModelList = new();
		public IReadOnlyDictionary<DiceModel, DiceView> ScreenDiceDict => screenDiceDict;
		public Dictionary<DiceModel, DiceView> screenDiceDict = new ();

		public void Setup(DiceGameConfig diceGameConfig, int maxBetSize)
		{
			SetMinBetSize(diceGameConfig.min_bet_size);
			SetMaxBetSize(maxBetSize);
			SetBetSize((diceGameConfig.min_bet_size + maxBetSize) / 2);
			SetTargetScore(diceGameConfig.target_score);
			SetCurrentTurn(1, true);
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
			foreach (var diceModel in CurrentDiceModelList)
			{
				diceModel.SetHide(false);
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
			TargetPoints = score;
			OnTargetPointsChanged?.Invoke();
		}

		public void IncreaseCurrentTurn()
		{
			CurrentTurn++;
			IsPlayerTurn = !IsPlayerTurn;
			OnCurrentTurnChanged?.Invoke();
		}

		public void SetCurrentTurn(int turn, bool isPlayerTurn)
		{
			CurrentTurn = turn;
			IsPlayerTurn = isPlayerTurn;
			OnCurrentTurnChanged?.Invoke();
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

		public void Reset()
		{
			CurrentDiceModelList.Clear();
			DiceGameState = DiceGameState.DEFAULT;
			IsDiceGameStarted = false;
			IsConditionPassed = false;
			CurrentTurn = 0;
		}
	}
}
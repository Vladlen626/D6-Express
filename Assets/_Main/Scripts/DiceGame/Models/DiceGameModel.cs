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
		public event Action OnMaxTurnCountChanged;
		public event Action OnCurrentTurnChanged;
		public event Action OnDiceGameStateChanged;

		public DiceGameState DiceGameState { get; private set; } = DiceGameState.DEFAULT;
		public int BetSize { get; private set; }
		public int MaxBetSize { get; private set; }
		public int MinBetSize { get; private set; }
		public int MaxTurnCount { get; private set; }
		public int CurrentTurn { get; private set; }
		public int TargetPoints { get; private set; }
		public bool IsConditionPassed { get; private set; }
		public bool IsDiceGameStarted { get; private set; }
		public readonly List<DiceModel> GameSelectedDiceModelsList = new();
		public IReadOnlyDictionary<DiceModel, DiceView> ScreenDiceDict => screenDiceDict;
		public Dictionary<DiceModel, DiceView> screenDiceDict = new ();

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
			foreach (var diceModel in GameSelectedDiceModelsList)
			{
				diceModel.SetHide(true);
			}
		}
		
		public void ShowAllDiceGameModels()
		{
			foreach (var diceModel in GameSelectedDiceModelsList)
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

		public void SetMaxTurnCount(int turn)
		{
			MaxTurnCount = turn;
			OnMaxTurnCountChanged?.Invoke();
		}

		public void IncreaseCurrentTurn()
		{
			CurrentTurn++;
			OnCurrentTurnChanged?.Invoke();
		}

		public void SetCurrentTurn(int turn)
		{
			CurrentTurn = turn;
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
			GameSelectedDiceModelsList.Clear();
			DiceGameState = DiceGameState.DEFAULT;
			IsDiceGameStarted = false;
			IsConditionPassed = false;
			CurrentTurn = 0;
		}
	}
}
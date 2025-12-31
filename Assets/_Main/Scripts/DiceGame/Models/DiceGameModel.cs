using System;

namespace _Main.Scripts.Dice
{
	public class DiceGameModel
	{
		public event Action OnGameConditionPassed;
		public event Action OnGameConditionFailed;
		public event Action OnBetSizeChanged;
		public event Action OnTargetPointsChanged;
		public event Action OnMaxTurnCountChanged;
		public event Action OnCurrentTurnChanged;

		public int BetSize { get; private set; }
		public int MaxTurnCount { get; private set; }
		public int CurrentTurn { get; private set; }
		public int TargetPoints { get; private set; }
		public bool IsConditionPassed { get; private set; }

		public void SetBetSize(int size)
		{
			BetSize = size;
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

		public void Reset()
		{
			IsConditionPassed = false;
			CurrentTurn = 0;
		}
	}
}
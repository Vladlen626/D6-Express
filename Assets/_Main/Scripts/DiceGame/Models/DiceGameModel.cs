using System;

namespace _Main.Scripts.Dice
{
	public class DiceGameModel
	{
		public event Action OnGameConditionPassed;
		public event Action OnGameConditionFailed;
		public event Action OnBetSizeChanged;
		public event Action OnTargetPointsChanged;

		public int BetSize { get; private set; }
		public int TargetPoints { get; private set; }
		public bool IsConditionPassed { get; private set; }

		public DiceGameModel()
		{
			IsConditionPassed = false;
		}

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
	}
}
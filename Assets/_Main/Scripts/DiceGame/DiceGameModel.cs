using System;

namespace _Main.Scripts.Dice
{
	public class DiceGameModel
	{
		public event Action OnBankedPointsChanged;
		public event Action OnGameConditionPassed;
		public int BankedPoints { get; private set; }

		private const int TARGET_SCORE = 4000;

		public void AddBankedPoints(int points)
		{
			BankedPoints += points;
			CheckPointsCondition();
			OnBankedPointsChanged?.Invoke();
		}

		public void Reset()
		{
			BankedPoints = 0;
		}


		private void CheckPointsCondition()
		{
			if (BankedPoints >= TARGET_SCORE)
			{
				OnGameConditionPassed?.Invoke();
			}
		}
	}
}
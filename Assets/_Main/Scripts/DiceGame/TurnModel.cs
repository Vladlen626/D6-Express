using System;

namespace _Main.Scripts.Dice
{
	public class TurnModel
	{
		public event Action OnTurnPointsChanged;
		public int TurnPoints { get; private set; }

		public void AddTurnPoints(int points)
		{
			TurnPoints += points;
			OnTurnPointsChanged?.Invoke();
		}

		public void Reset()
		{
			TurnPoints = 0;
		}
	}
}
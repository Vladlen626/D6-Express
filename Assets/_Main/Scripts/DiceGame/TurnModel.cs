using System;

namespace _Main.Scripts.Dice
{
	public class TurnModel
	{
		public event Action OnTurnPointsChanged;
		public int TurnPoints { get; private set; }

		public void AddTurnPoints(int points)
		{
			SetTurnPoints(TurnPoints + points);
		}

		public void Reset()
		{
			SetTurnPoints(0);
		}
		
		private void SetTurnPoints(int points)
		{
			TurnPoints = points;
			OnTurnPointsChanged?.Invoke();
		}
	}
}
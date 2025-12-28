using System;

namespace _Main.Scripts.Dice
{
	public class TurnModel
	{
		public event Action OnTurnPointsChanged;
		public event Action OnPreviewPointsChanged;
		public int TurnPoints { get; private set; }
		public int PreviewPoints { get; private set; }


		public void SetPreviewPoints(int points)
		{
			PreviewPoints = points;
			OnPreviewPointsChanged?.Invoke();
		}
		public void AddTurnPoints(int points)
		{
			SetTurnPoints(TurnPoints + points);
		}

		public void Reset()
		{
			SetTurnPoints(0);
			SetPreviewPoints(0);
		}
		
		private void SetTurnPoints(int points)
		{
			TurnPoints = points;
			OnTurnPointsChanged?.Invoke();
		}
	}
}
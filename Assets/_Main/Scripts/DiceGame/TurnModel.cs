namespace _Main.Scripts.Dice
{
	public class TurnModel
	{
		public int TurnPoints { get; private set; }

		public void AddTurnPoints(int points)
		{
			TurnPoints += points;
		}

		public void Reset()
		{
			TurnPoints = 0;
		}
	}
}
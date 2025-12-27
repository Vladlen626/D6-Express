namespace _Main.Scripts.Dice
{
	public class GameModel
	{
		public const int TARGET_SCORE = 4000;

		public int BankedPoints { get; private set; }

		public void AddBankedPoints(int points)
		{
			BankedPoints += points;
		}

		public void Reset()
		{
			BankedPoints = 0;
		}
	}
}
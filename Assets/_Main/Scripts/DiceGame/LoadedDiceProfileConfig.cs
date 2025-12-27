namespace _Main.Scripts.Dice
{
	public class LoadedDiceProfileConfig
	{
		private string profileName = "Default";
		private float[] weights = new float[6] { 1, 1, 1, 1, 1, 1 };

		public string ProfileName => profileName;
		public float[] Weights => weights;
	}

}
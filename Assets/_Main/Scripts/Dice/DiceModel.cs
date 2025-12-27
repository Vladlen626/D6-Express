namespace _Main.Scripts.Dice
{
	public class LoadedDiceProfileConfig
	{
		private string profileName = "Default";
		private float[] weights = new float[6] { 1, 1, 1, 1, 1, 1 };

		public string ProfileName => profileName;
		public float[] Weights => weights;
	}

	public class DiceModel
	{
		public int CurrentValue { get; private set; }
		public bool IsChosen { get; private set; }
		public bool IsSaved { get; private set; }

		public LoadedDiceProfileConfig Profile { get; private set; }

		public DiceModel(LoadedDiceProfileConfig profile)
		{
			Profile = profile;
			CurrentValue = 0;
		}

		public void SetValue(int value)
		{
			CurrentValue = value;
		}

		public void SetChosen(bool chosen)
		{
			IsChosen = chosen;
		}

		public void SetSaved(bool saved)
		{
			IsSaved = saved;
		}

		public void Reset()
		{
			CurrentValue = 0;
			IsChosen = false;
			IsSaved = false;
		}
	}
}
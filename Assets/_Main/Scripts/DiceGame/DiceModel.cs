using System;

namespace _Main.Scripts.Dice
{
	public class DiceModel
	{
		public event Action OnDiceChosenChanged;
		public event Action OnDiceSavedChanged;
		public event Action OnValueChanged;
		public int CurrentValue { get; private set; }
		public bool IsChosen { get; private set; }
		public bool IsSaved { get; private set; }

		public LoadedDiceProfileConfig Profile { get; private set; }

		public DiceModel(LoadedDiceProfileConfig profile)
		{
			Profile = profile;
			CurrentValue = 0;
		}

		public void Roll()
		{
			var newValue = DiceGameUtils.GetWeightedRandomValue(Profile.Weights);
			SetValue(newValue);
		}

		public void SetValue(int value)
		{
			CurrentValue = value;
			OnValueChanged?.Invoke();
		}

		public void SetChosen(bool chosen)
		{
			IsChosen = chosen;
			OnDiceChosenChanged?.Invoke();
		}

		public void SetSaved(bool saved)
		{
			IsSaved = saved;
			OnDiceSavedChanged?.Invoke();
		}

		public void Reset()
		{
			CurrentValue = 0;
			IsChosen = false;
			IsSaved = false;
		}
	}
}
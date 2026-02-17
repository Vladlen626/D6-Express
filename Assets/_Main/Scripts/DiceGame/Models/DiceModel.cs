using System;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class DiceModel
	{
		public event Action OnDiceHiddenChanged;
		public event Action OnDiceChosenChanged;
		public event Action<bool, bool> OnDiceSavedChanged;
		public event Action OnValueChanged;
		public int CurrentValue { get; private set; }
		public bool IsHide { get; private set; }
		public bool IsChosen { get; private set; }
		public bool IsSaved { get; private set; }
		public Transform CurrentPosition { get; private set; }
		public int[] Weights { get; private set; }
		public string ConfigId { get; private set; }

		public DiceModel(string configId, int[] weights)
		{
			ConfigId = configId;
			Weights = weights;
			Reset();
		}

		public void Roll()
		{
			var newValue = DiceGameUtils.GetWeightedRandomValue(Weights);
			//var newValue = 1;
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
			var oldIsSaved = IsSaved;
			IsSaved = saved;
			OnDiceSavedChanged?.Invoke(oldIsSaved, IsSaved);
		}

		public void SetHide(bool hide)
		{
			IsHide = hide;
			OnDiceHiddenChanged?.Invoke();
		}

		public void SetCurrentPosition(Transform position)
		{
			CurrentPosition = position;
		}

		public void Reset()
		{
			SetChosen(false);
			SetSaved(false);
			SetValue(0);
		}
	}
}

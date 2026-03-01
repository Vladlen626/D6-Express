using System;
using System.Collections.Generic;
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
		private readonly Queue<int> forcedRollValues = new();

		public DiceModel(string configId, int[] weights)
		{
			ConfigId = configId;
			Weights = weights;
			Reset();
		}

		public void Roll()
		{
			var newValue = forcedRollValues.Count > 0
				? forcedRollValues.Dequeue()
				: DiceGameUtils.GetWeightedRandomValue(Weights);
			SetValue(newValue);
		}

		public void EnqueueForcedRollValue(int value)
		{
			if (value < 1 || value > 6)
			{
				return;
			}

			forcedRollValues.Enqueue(value);
		}

		public void ClearForcedRollValues()
		{
			forcedRollValues.Clear();
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
			ClearForcedRollValues();
			SetChosen(false);
			SetSaved(false);
			SetValue(0);
		}
	}
}

using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class DiceController : IBaseController, IActivatable, IDeactivatable
	{
		private readonly DiceModel diceModel;
		private readonly DiceView diceView;

		public DiceController(DiceModel inDiceModel, DiceView inDiceView)
		{
			diceModel = inDiceModel;
			diceView = inDiceView;
		}
		public void Activate()
		{
			Roll();
			diceView.OnDiceClicked += HandleDiceClicked;
		}

		public void Deactivate()
		{
			if (diceView != null)
			{
				diceView.OnDiceClicked -= HandleDiceClicked;
			}
		}

		public void Roll()
		{
			float[] weights = diceModel.Profile.Weights;
			int value = GetWeightedRandomValue(weights);

			diceModel.SetValue(value);
			diceView.SetSideMesh(value);
		}

		private void HandleDiceClicked()
		{
			if (diceModel.IsSaved) return;

			diceModel.SetChosen(!diceModel.IsChosen);
			diceView.UpdateChosenVisual(diceModel.IsChosen);
		}

		public void Save()
		{
			diceModel.SetSaved(true);
			diceModel.SetChosen(false);
			diceView.UpdateChosenVisual(false);
		}

		public void Reset()
		{
			diceModel.Reset();
			diceView.SetSideMesh(0);
			diceView.UpdateChosenVisual(false);
		}

		private int GetWeightedRandomValue(float[] weights)
		{
			float totalWeight = 0f;
			foreach (float weight in weights)
			{
				totalWeight += weight;
			}

			float randomValue = Random.Range(0f, totalWeight);
			float cumulativeWeight = 0f;

			for (int i = 0; i < weights.Length; i++)
			{
				cumulativeWeight += weights[i];
				if (randomValue <= cumulativeWeight)
				{
					return i + 1;
				}
			}

			return 1;
		}
	}
}
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class AdjustUnsavedDieValueModifier : IOnRollModifier, IModifierUiConfigProvider, IModifierApplyResultProvider
	{
		private readonly DiceScoringService scoringService;
		public readonly UnsavedDieSelectionStrategy selectionStrategy;
		public readonly int delta;
		public readonly int minValue;
		public readonly int maxValue;
		public string UiConfigId { get; }
		public bool LastApplyHadEffect { get; private set; }

		public AdjustUnsavedDieValueModifier(
			DiceScoringService scoringService,
			UnsavedDieSelectionStrategy selectionStrategy,
			int delta,
			int minValue = 1,
			int maxValue = 6,
			string uiConfigId = null)
		{
			if (minValue > maxValue)
			{
				throw new ArgumentException("minValue must be less than or equal to maxValue.");
			}

			this.scoringService = scoringService;
			this.selectionStrategy = selectionStrategy;
			this.delta = delta;
			this.minValue = minValue;
			this.maxValue = maxValue;
			UiConfigId = uiConfigId;
		}

		public UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			LastApplyHadEffect = false;
			if (scoringService == null || delta == 0)
			{
				return UniTask.CompletedTask;
			}

			var selectedDice = RollModifierUtils.SelectUnsavedDie(modifierContext, selectionStrategy);
			if (selectedDice == null)
			{
				return UniTask.CompletedTask;
			}

			var nextValue = Mathf.Clamp(selectedDice.CurrentValue + delta, minValue, maxValue);
			if (nextValue == selectedDice.CurrentValue)
			{
				return UniTask.CompletedTask;
			}

			selectedDice.SetValue(nextValue);
			LastApplyHadEffect = true;
			RollModifierUtils.RefreshCombinationResult(modifierContext, scoringService);
			return UniTask.CompletedTask;
		}
	}
}

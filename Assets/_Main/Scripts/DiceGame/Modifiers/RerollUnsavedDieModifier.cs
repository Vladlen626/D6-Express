using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class RerollUnsavedDieModifier : IOnRollModifier, IModifierUiConfigProvider, IModifierApplyResultProvider
	{
		private readonly DiceScoringService scoringService;
		private readonly float chance;
		private readonly float rerollAnimationDuration;

		public readonly UnsavedDieSelectionStrategy selectionStrategy;
		public string UiConfigId { get; }
		public bool LastApplyHadEffect { get; private set; }

		public RerollUnsavedDieModifier(
			DiceScoringService scoringService,
			UnsavedDieSelectionStrategy selectionStrategy,
			float chance = 1f,
			float rerollAnimationDuration = 0.35f,
			string uiConfigId = null)
		{
			this.scoringService = scoringService;
			this.selectionStrategy = selectionStrategy;
			this.chance = Mathf.Clamp01(chance);
			this.rerollAnimationDuration = Mathf.Max(0.05f, rerollAnimationDuration);
			UiConfigId = uiConfigId;
		}

		public async UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			LastApplyHadEffect = false;
			if (scoringService == null)
			{
				return;
			}

			var selectedDice = RollModifierUtils.SelectUnsavedDie(modifierContext, selectionStrategy);
			if (selectedDice == null)
			{
				return;
			}

			if (Random.value > chance)
			{
				return;
			}

			selectedDice.Roll();
			LastApplyHadEffect = true;

			if (modifierContext.DiceGameModel != null &&
			    modifierContext.DiceGameModel.ScreenDiceDict.TryGetValue(selectedDice, out var view))
			{
				await view.PlayRollAnimationAsync(rerollAnimationDuration);
			}

			RollModifierUtils.RefreshCombinationResult(modifierContext, scoringService);
		}
	}
}

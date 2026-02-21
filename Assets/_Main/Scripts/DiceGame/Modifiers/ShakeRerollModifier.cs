using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class ShakeRerollModifier : ModifierItemBase, IOnRollModifier
	{
		private readonly DiceScoringService scoringService;
		public readonly float shakeChance;
		private readonly float rerollAnimationDuration;

		public ShakeRerollModifier(string id, DiceScoringService scoringService, float shakeChance = 0.95f, float rerollAnimationDuration = 0.5f)
			: base(id, id, DiceItemActivationType.Passive)
		{
			this.scoringService = scoringService;
			this.shakeChance = Mathf.Clamp01(shakeChance);
			this.rerollAnimationDuration = Mathf.Max(0.05f, rerollAnimationDuration);
		}

		public override async UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			if (modifierContext.Dice == null || modifierContext.Dice.Length == 0)
			{
				return;
			}

			var availableDice = modifierContext.Dice.Where(d => d != null && !d.IsSaved).ToArray();
			if (availableDice.Length == 0)
			{
				return;
			}

			if (Random.value > shakeChance)
			{
				return;
			}

			var shakenDice = availableDice[Random.Range(0, availableDice.Length)];
			shakenDice.Roll();

			if (modifierContext.DiceGameModel.ScreenDiceDict.TryGetValue(shakenDice, out var view))
			{
				await view.PlayRollAnimationAsync(rerollAnimationDuration);
			}

			RefreshCombinationResult(modifierContext);
		}

		private void RefreshCombinationResult(DiceModifierContext modifierContext)
		{
			var updatedResult = scoringService.Evaluate(DiceGameUtils.GetDiceValues(modifierContext.Dice));
			var targetList = modifierContext.CombinationResult.Combinations;
			targetList.Clear();
			targetList.AddRange(updatedResult.Combinations);
		}
	}
}

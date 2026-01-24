using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Clickable item that lets the player reroll their selected dice once on the next Pass.
	/// After activation it goes on cooldown for a set number of Pass actions (default: 2).
	/// </summary>
	public class RerollSelectedItem : DiceItemBase, IOnPassModifier, IOnRoundStartModifier, IDiceItemViewProvider
	{
		private readonly int cooldownLengthInPasses;
		private readonly DiceItemView customPrefab;
		private int cooldownRemaining;

		public RerollSelectedItem(int cooldownPasses = 2, DiceItemView prefabOverride = null)
			: base("reroll_selected_item", "Second Chance", DiceItemActivationType.ClickToActivate)
		{
			cooldownLengthInPasses = Mathf.Max(1, cooldownPasses);
			customPrefab = prefabOverride;
		}

		public override UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			switch (modifierContext.Stage)
			{
				case ModifierStage.RoundStart:
					// If a new round starts while armed, keep it armed; cooldown is tracked per pass only.
					break;

				case ModifierStage.Pass:
					TickCooldown();
					if (State == DiceItemState.Armed)
					{
						ApplyReroll(modifierContext);
						BeginCooldown();
					}
					break;
			}

			return UniTask.CompletedTask;
		}

		protected override bool OnClick()
		{
			if (State != DiceItemState.Ready)
			{
				return false;
			}

			SetState(DiceItemState.Armed);
			return true;
		}

		private void ApplyReroll(DiceModifierContext context)
		{
			if (context.Dice == null || context.Dice.Length == 0 || context.CombinationResult == null)
			{
				return;
			}

			foreach (var dice in context.Dice)
			{
				if (dice == null || dice.IsSaved)
				{
					continue;
				}

				dice.Roll();
			}

			// Refresh combinations to reflect the new dice values.
			var recomputed = DiceGameUtils.GetCombinations(DiceGameUtils.GetDiceValues(context.Dice));
			var targetList = context.CombinationResult.Combinations;
			targetList.Clear();
			targetList.AddRange(recomputed.Combinations);
		}

		private void BeginCooldown()
		{
			cooldownRemaining = cooldownLengthInPasses;
			StartCooldown();
		}

		private void TickCooldown()
		{
			if (State != DiceItemState.Cooldown)
			{
				return;
			}

			if (cooldownRemaining > 0)
			{
				cooldownRemaining--;
			}

			if (cooldownRemaining <= 0)
			{
				SetState(DiceItemState.Ready);
			}
		}

		public override void ResetItem()
		{
			base.ResetItem();
			cooldownRemaining = 0;
		}

		public DiceItemView GetViewPrefab() => customPrefab;
	}
}

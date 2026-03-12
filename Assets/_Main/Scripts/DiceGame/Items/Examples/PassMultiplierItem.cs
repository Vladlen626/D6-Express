using Cysharp.Threading.Tasks;
using _Main.Scripts.Core;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Example clickable item: when armed, the next Pass gains a score multiplier.
	/// After the first successful activation, the item is consumed.
	/// </summary>
	public class PassMultiplierItem : ModifierItemBase, IOnPassModifier, IModifierItemViewProvider
	{
		private readonly float scoreMultiplier;
		private readonly ItemView customPrefab;

		public PassMultiplierItem(string id, float scoreMultiplier = 1.5f, int activationsPerDay = 1, ItemView prefabOverride = null)
			: base(id, id, DiceItemActivationType.ClickToActivate)
		{
			this.scoreMultiplier = Mathf.Max(1f, scoreMultiplier);
			_ = activationsPerDay;
			customPrefab = prefabOverride;
		}

		public override string InvalidActivationNotificationKey => GlobalConstants.Localization.ItemActivationOnlyGame;

		public override bool IsActivationAllowed(DiceGameState gameState)
		{
			return gameState == DiceGameState.GAME;
		}

		public override UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			if (modifierContext.Stage == ModifierStage.Pass)
			{
				ApplyIfArmed(modifierContext.CombinationResult);
			}

			return UniTask.CompletedTask;
		}

		protected override bool OnClick()
		{
			if (State == DiceItemState.Ready)
			{
				SetState(DiceItemState.Armed);
				NotifyActivationStarted();
				return true;
			}

			return false;
		}

		private void ApplyIfArmed(DiceCombinationResult combinationResult)
		{
			if (State != DiceItemState.Armed || combinationResult.Combinations == null)
			{
				return;
			}

			foreach (var entry in combinationResult.Combinations)
			{
				entry.BaseScore = Mathf.RoundToInt(entry.BaseScore * scoreMultiplier);
			}

			NotifyEffectApplied();
			Consume();
		}

		public override void ResetItem()
		{
			base.ResetItem();
		}

		public ItemView GetViewPrefab()
		{
			return customPrefab;
		}
	}
}

using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Match-long temporary target score discount. Applies on click and is consumed at match end.
	/// </summary>
	public class TargetDiscountItem : ModifierItemBase, IGameModelBoundItem, IModifierItemViewProvider, IOnMatchFinishedItem
	{
		private readonly int bonus;
		private readonly ItemView customPrefab;
		private DiceGameModel boundGameModel;
		private int appliedDelta;
		private bool isApplied;

		public TargetDiscountItem(string id, int bonus = -300, ItemView prefabOverride = null)
			: base(id, id, DiceItemActivationType.ClickToActivate)
		{
			this.bonus = Mathf.Min(-1, bonus);
			customPrefab = prefabOverride;
		}

		public override string InvalidActivationNotificationKey => GlobalConstants.Localization.ItemActivationOnlySelectDice;

		public override bool IsActivationAllowed(DiceGameState gameState)
		{
			return gameState == DiceGameState.SELECT_DICE;
		}

		protected override bool OnClick()
		{
			if (State != DiceItemState.Ready || boundGameModel == null || isApplied)
			{
				return false;
			}

			SetState(DiceItemState.Armed);
			NotifyActivationStarted();

			var before = boundGameModel.TargetPoints;
			var after = Mathf.Max(0, before + bonus);
			appliedDelta = after - before;
			boundGameModel.SetTargetScore(after);
			isApplied = true;

			NotifyEffectApplied();
			return true;
		}

		public void OnMatchFinished()
		{
			RemoveAppliedDiscount();
			if (State == DiceItemState.Armed)
			{
				Consume();
			}
		}

		public override UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			return UniTask.CompletedTask;
		}

		public void OnAddedToGameModel(DiceGameModel gameModel)
		{
			boundGameModel = gameModel;
		}

		public void OnRemovedFromGameModel(DiceGameModel gameModel)
		{
			RemoveAppliedDiscount();
			if (object.ReferenceEquals(boundGameModel, gameModel))
			{
				boundGameModel = null;
			}
		}

		private void RemoveAppliedDiscount()
		{
			if (!isApplied || boundGameModel == null || appliedDelta == 0)
			{
				return;
			}

			var target = Mathf.Max(0, boundGameModel.TargetPoints - appliedDelta);
			boundGameModel.SetTargetScore(target);
			appliedDelta = 0;
			isApplied = false;
		}

		public override void ResetItem()
		{
			RemoveAppliedDiscount();
			base.ResetItem();
			boundGameModel = null;
			appliedDelta = 0;
			isApplied = false;
		}

		public ItemView GetViewPrefab() => customPrefab;
	}
}

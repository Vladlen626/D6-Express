using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using System;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Connects an ItemView with a modifier item and feeds state changes both ways.
	/// Register this controller in the lifecycle to hook/unhook automatically.
	/// </summary>
	public class ModifierItemController : IBaseController, IActivatable
	{
		private readonly IModifierItem item;
		private readonly ItemView view;
		private readonly DiceGameModel diceGameModel;
		private readonly DiceItemViewRegistry itemViewRegistry;
		private readonly GlobalNotificationService notificationService;

		public ModifierItemController(
			IModifierItem item,
			ItemView view,
			DiceGameModel diceGameModel,
			DiceItemViewRegistry itemViewRegistry,
			GlobalNotificationService notificationService)
		{
			this.item = item ?? throw new ArgumentNullException(nameof(item));
			this.view = view ? view : throw new ArgumentNullException(nameof(view));
			this.diceGameModel = diceGameModel ?? throw new ArgumentNullException(nameof(diceGameModel));
			this.itemViewRegistry = itemViewRegistry ?? throw new ArgumentNullException(nameof(itemViewRegistry));
			this.notificationService = notificationService;
		}

		public void Activate()
		{
			item.AttachView(view);
			view.OnClicked.AddListener(OnViewClicked);
			item.OnChanged += OnItemChanged;
			itemViewRegistry.Register(item, view);

			diceGameModel.OnDiceGameStateChanged += OnDiceGameStateChanged;
			diceGameModel.OnActiveTargetingItemChanged += OnActiveTargetingItemChanged;

			SyncTargetingState();
			RefreshPhaseDisabledVisual();
		}

		public void Deactivate()
		{
			diceGameModel.OnActiveTargetingItemChanged -= OnActiveTargetingItemChanged;
			diceGameModel.OnDiceGameStateChanged -= OnDiceGameStateChanged;
			diceGameModel.ClearItemTargeting(item);
			itemViewRegistry.Unregister(item, view);

			item.OnChanged -= OnItemChanged;
			view.OnClicked.RemoveListener(OnViewClicked);
			item.DetachView();
		}

		private void OnViewClicked()
		{
			if (TryCancelActiveTargetingOnSelfClick())
			{
				return;
			}

			if (IsInteractionBlocked())
			{
				if (IsActivationPhaseBlocked() && !string.IsNullOrWhiteSpace(item.InvalidActivationNotificationKey))
				{
					notificationService?.ShowToastImmediate(item.InvalidActivationNotificationKey, true);
				}

				return;
			}

			if (!item.TryHandleClick())
			{
				return;
			}

			SyncTargetingState();
			RefreshPhaseDisabledVisual();
		}

		private bool TryCancelActiveTargetingOnSelfClick()
		{
			if (!object.ReferenceEquals(diceGameModel.ActiveTargetingItem, item))
			{
				return false;
			}

			if (item is not IArmedTargetingItem targetingItem || !targetingItem.IsAwaitingTargetSelection)
			{
				return false;
			}

			if (!targetingItem.TryCancelArmedTargeting())
			{
				return false;
			}

			SyncTargetingState();
			RefreshPhaseDisabledVisual();
			return true;
		}

		private void OnDiceGameStateChanged()
		{
			RefreshPhaseDisabledVisual();
		}

		private void OnActiveTargetingItemChanged(IModifierItem _)
		{
			RefreshPhaseDisabledVisual();
		}

		private void OnItemChanged(IModifierItem _)
		{
			SyncTargetingState();
			RefreshPhaseDisabledVisual();
		}

		private void RefreshPhaseDisabledVisual()
		{
			if (!view)
			{
				return;
			}

			var shouldDisable = item.State == DiceItemState.Ready && IsInteractionBlocked();
			view.SetPhaseDisabled(shouldDisable);
		}

		private bool IsInteractionBlocked()
		{
			if (item.ActivationType != DiceItemActivationType.ClickToActivate)
			{
				return false;
			}

			if (diceGameModel.IsItemTargetingActive &&
			    !object.ReferenceEquals(diceGameModel.ActiveTargetingItem, item))
			{
				return true;
			}

			if (diceGameModel.IsDiceAnimationInProgress)
			{
				return true;
			}

			if (item.State != DiceItemState.Ready)
			{
				return false;
			}

			if (!diceGameModel.IsPlayerTurn)
			{
				return true;
			}

			return !item.IsActivationAllowed(diceGameModel.DiceGameState);
		}

		private bool IsActivationPhaseBlocked()
		{
			return item.ActivationType == DiceItemActivationType.ClickToActivate &&
			       item.State == DiceItemState.Ready &&
			       !item.IsActivationAllowed(diceGameModel.DiceGameState);
		}

		private void SyncTargetingState()
		{
			if (item is not IArmedTargetingItem targetingItem)
			{
				diceGameModel.ClearItemTargeting(item);
				return;
			}

			if (targetingItem.IsAwaitingTargetSelection)
			{
				if (!diceGameModel.TryStartItemTargeting(item))
				{
					targetingItem.TryCancelArmedTargeting();
				}
				return;
			}

			diceGameModel.ClearItemTargeting(item);
		}
	}
}

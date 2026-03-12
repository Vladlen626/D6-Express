using System;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Base implementation for dice items that double as modifiers.
	/// Derive from this and implement ModifyValues to plug into the modifier pipeline.
	/// </summary>
	public abstract class ModifierItemBase : IModifierItem
	{
		public string Id { get; }
		public string DisplayName { get; }
		public DiceItemActivationType ActivationType { get; }
		public DiceItemState State { get; protected set; } = DiceItemState.Ready;
		public bool IsVisible { get; protected set; } = true;
		public virtual string InvalidActivationNotificationKey => string.Empty;

		protected DiceItemView AttachedView { get; private set; }

		public event Action<IModifierItem> OnChanged;

		protected ModifierItemBase(string id, string displayName, DiceItemActivationType activationType)
		{
			Id = id;
			DisplayName = displayName;
			ActivationType = activationType;
		}

		public void AttachView(DiceItemView view)
		{
			AttachedView = view;
			AttachedView.Bind(this);
			UpdateView();
		}

		public void DetachView()
		{
			if (!AttachedView)
			{
				return;
			}

			AttachedView.Unbind(this);
			AttachedView = null;
		}

		public bool TryHandleClick()
		{
			if (ActivationType == DiceItemActivationType.Passive || State is DiceItemState.Disabled or DiceItemState.Cooldown or DiceItemState.Consumed)
			{
				return false;
			}

			return OnClick();
		}

		public virtual bool IsActivationAllowed(DiceGameState gameState)
		{
			return true;
		}

		/// <summary>
		/// Override to perform arming or immediate effect on click. Return true if handled.
		/// </summary>
		protected virtual bool OnClick()
		{
			if (State == DiceItemState.Ready)
			{
				SetState(DiceItemState.Armed);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Helper to move the item into cooldown after use.
		/// </summary>
		protected void StartCooldown()
		{
			SetState(DiceItemState.Cooldown);
		}

		/// <summary>
		/// Helper to mark the item fully consumed (e.g., single-use items).
		/// </summary>
		protected void Consume()
		{
			SetState(DiceItemState.Consumed);
		}

		protected void SetState(DiceItemState newState)
		{
			if (State == newState)
			{
				return;
			}

			State = newState;
			NotifyChanged();
			UpdateView();
		}

		protected void SetVisibility(bool visible)
		{
			if (IsVisible == visible)
			{
				return;
			}

			IsVisible = visible;
			NotifyChanged();
			UpdateView();
		}

		protected void NotifyChanged()
		{
			OnChanged?.Invoke(this);
		}

		protected void UpdateView()
		{
			AttachedView?.UpdateState(State, IsVisible);
		}

		public virtual void ResetItem()
		{
			State = DiceItemState.Ready;
			IsVisible = true;
			UpdateView();
			NotifyChanged();
		}

		public abstract Cysharp.Threading.Tasks.UniTask ModifyValues(DiceModifierContext modifierContext);
	}
}

using System;

namespace _Main.Scripts.Dice
{
	public enum DiceItemActivationType
	{
		Passive,
		ClickToActivate
	}

	public enum DiceItemState
	{
		Hidden,
		Ready,
		Armed,
		Cooldown,
		Consumed,
		Disabled
	}

	/// <summary>
	/// A modifier item is a modifier with a physical (3D) representation. It can be passive or require a click to arm/activate.
	/// </summary>
	public interface IModifierItem : IModifier
	{
		string Id { get; }
		string DisplayName { get; }
		DiceItemActivationType ActivationType { get; }
		DiceItemState State { get; }
		bool IsVisible { get; }

		event Action<IModifierItem> OnChanged;

		/// <summary>
		/// Attach a view so the item can update its 3D representation (color/highlight, visibility, etc.).
		/// </summary>
		void AttachView(DiceItemView view);
		void DetachView();

		/// <summary>
		/// Called by the view/controller when the player clicks the 3D model.
		/// Should arm or trigger the item depending on its activation type.
		/// </summary>
		bool TryHandleClick();

		/// <summary>
		/// Restore the item to its initial state (e.g., at run start or after cleanup).
		/// </summary>
		void ResetItem();
	}

	/// <summary>
	/// Optional: implement to supply a custom view prefab per item.
	/// </summary>
	public interface IModifierItemViewProvider
	{
		DiceItemView GetViewPrefab();
	}

	/// <summary>
	/// Optional match lifecycle hook for item models that need to finalize state at match end.
	/// </summary>
	public interface IOnMatchFinishedItem
	{
		void OnMatchFinished();
	}
}

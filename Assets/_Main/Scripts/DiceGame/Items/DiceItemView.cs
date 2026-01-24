using System;
using UnityEngine;
using UnityEngine.Events;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Minimal 3D view for an item. Handles click detection and simple state visualization.
	/// Swap the visuals/colors in the inspector to match your art style.
	/// </summary>
	public class DiceItemView : MonoBehaviour
	{
		[SerializeField] private Collider clickCollider;
		[SerializeField] private Renderer[] renderers;
		[SerializeField] private Color readyColor = Color.white;
		[SerializeField] private Color armedColor = Color.green;
		[SerializeField] private Color cooldownColor = Color.gray;
		[SerializeField] private Color consumedColor = Color.black;
		[SerializeField] private Color disabledColor = Color.red;

		private IDiceItem boundItem;
		public UnityEvent OnClicked = new();

		public void Bind(IDiceItem item)
		{
			if (boundItem == item)
			{
				return;
			}

			Unbind(boundItem);
			boundItem = item;
			if (boundItem != null)
			{
				boundItem.OnChanged += OnItemChanged;
			}
		}

		public void Unbind(IDiceItem item)
		{
			if (item == null || boundItem != item)
			{
				return;
			}

			boundItem.OnChanged -= OnItemChanged;
			boundItem = null;
		}

		private void OnDestroy()
		{
			if (boundItem != null)
			{
				boundItem.OnChanged -= OnItemChanged;
				boundItem = null;
			}
			OnClicked.RemoveAllListeners();
		}

		private void OnMouseDown()
		{
			if (boundItem == null || clickCollider == null)
			{
				return;
			}

			OnClicked.Invoke();
		}

		private void OnItemChanged(IDiceItem item)
		{
			UpdateState(item.State, item.IsVisible);
		}

		public void UpdateState(DiceItemState state, bool isVisible)
		{
			gameObject.SetActive(isVisible);
			var color = readyColor;
			switch (state)
			{
				case DiceItemState.Armed:
					color = armedColor;
					break;
				case DiceItemState.Cooldown:
					color = cooldownColor;
					break;
				case DiceItemState.Consumed:
					color = consumedColor;
					break;
				case DiceItemState.Disabled:
					color = disabledColor;
					break;
				case DiceItemState.Ready:
				case DiceItemState.Hidden:
					color = readyColor;
					break;
			}

			foreach (var r in renderers)
			{
				if (r == null) continue;
				r.material.color = color;
			}
		}
	}
}

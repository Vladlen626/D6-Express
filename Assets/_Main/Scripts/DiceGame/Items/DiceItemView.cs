using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

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
		private Camera _cam;

		private void Awake()
		{
			_cam = Camera.main;
		}

		public void Bind(IDiceItem item)
		{
			Debug.Log($"[DiceItemView] Bind -> {(item != null ? item.Id : "null")}");
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

			Debug.Log($"[DiceItemView] Unbind -> {item.Id}");
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

		private void Update()
		{
			if (boundItem == null || !clickCollider)
			{
				return;
			}

			var mouse = Mouse.current;
			if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
			{
				return;
			}

			if (!_cam)
			{
				return;
			}

			Ray ray = _cam.ScreenPointToRay(mouse.position.ReadValue());
			if (clickCollider.Raycast(ray, out _, 200f))
			{
				Debug.Log($"[DiceItemView] Click detected on {boundItem.Id}");
				OnClicked.Invoke();
			}
		}

		private void OnItemChanged(IDiceItem item)
		{
			Debug.Log($"[DiceItemView] OnItemChanged -> {item.Id} state={item.State} visible={item.IsVisible}");
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
				if (!r)
				{
					continue;
				}
				r.material.color = color;
			}
		}
	}
}

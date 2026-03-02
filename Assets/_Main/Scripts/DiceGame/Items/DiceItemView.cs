using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Minimal 3D view for an item. Handles click detection and visibility.
	/// </summary>
	public class DiceItemView : MonoBehaviour
	{
		[SerializeField] private Collider clickCollider;

		private IModifierItem boundItem;
		public UnityEvent OnClicked = new();
		public event Action<IModifierItem> OnHoverEnter;
		public event Action<IModifierItem> OnHoverExit;
		private Camera _cam;
		private bool _isHovered;

		private void Awake()
		{
			_cam = Camera.main;
		}

		public void Bind(IModifierItem item)
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

		public void Unbind(IModifierItem item)
		{
			if (item == null || boundItem != item)
			{
				return;
			}

			if (_isHovered)
			{
				_isHovered = false;
				OnHoverExit?.Invoke(boundItem);
			}

			Debug.Log($"[DiceItemView] Unbind -> {item.Id}");
			boundItem.OnChanged -= OnItemChanged;
			boundItem = null;
		}

		private void OnDestroy()
		{
			if (boundItem != null)
			{
				if (_isHovered)
				{
					_isHovered = false;
					OnHoverExit?.Invoke(boundItem);
				}

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
			if (mouse == null)
			{
				return;
			}

			var isMouseOver = IsMouseOver(mouse);
			if (isMouseOver && !_isHovered)
			{
				_isHovered = true;
				OnHoverEnter?.Invoke(boundItem);
			}
			else if (!isMouseOver && _isHovered)
			{
				_isHovered = false;
				OnHoverExit?.Invoke(boundItem);
			}

			if (!mouse.leftButton.wasPressedThisFrame)
			{
				return;
			}

			if (!_cam)
			{
				return;
			}

			if (isMouseOver)
			{
				Debug.Log($"[DiceItemView] Click detected on {boundItem.Id}");
				OnClicked.Invoke();
			}
		}

		private bool IsMouseOver(Mouse mouse)
		{
			if (!_cam || !clickCollider)
			{
				return false;
			}

			Ray ray = _cam.ScreenPointToRay(mouse.position.ReadValue());
			return clickCollider.Raycast(ray, out _, 200f);
		}

		private void OnItemChanged(IModifierItem item)
		{
			Debug.Log($"[DiceItemView] OnItemChanged -> {item.Id} state={item.State} visible={item.IsVisible}");
			UpdateState(item.State, item.IsVisible);
		}

		public void UpdateState(DiceItemState state, bool isVisible)
		{
			if (!isVisible && _isHovered)
			{
				_isHovered = false;
				if (boundItem != null)
				{
					OnHoverExit?.Invoke(boundItem);
				}
			}

			gameObject.SetActive(isVisible);
		}
	}
}

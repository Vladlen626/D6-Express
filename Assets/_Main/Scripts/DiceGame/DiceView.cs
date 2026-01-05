using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.Events;

namespace _Main.Scripts.Dice
{
	public class DiceView : MonoBehaviour
	{
		[SerializeField] 
		private List<DiceVisualEntry> diceVisuals;
		
		[SerializeField] private Transform model;
		[SerializeField] private Outline outline;
		[SerializeField] private Collider diceCollider;

		[SerializeField] private float animSpeed = 0.15f;
		[SerializeField] private float yOffset = 0.02f;

		[HideInInspector]
		public UnityEvent OnDiceClicked;

		private Dictionary<string, Transform> _diceVisualMap;
		private Camera _mainCamera;
		private bool _isPressed;

		public void Initialize(string diceConfigId)
		{
			_diceVisualMap = new Dictionary<string, Transform>();
			foreach (var entry in diceVisuals)
			{
				if (!_diceVisualMap.ContainsKey(entry.id))
				{
					_diceVisualMap.Add(entry.id, entry.visual);
				}
			}

			_mainCamera = Camera.main;
			SetupVisual(diceConfigId);
		}

		private void Update()
		{
			if (!_mainCamera)
			{
				return;
			}
			
			if (Mouse.current.leftButton.wasPressedThisFrame)
			{
				if (IsMouseOverDice())
				{
					_isPressed = true;
					PlayPressAnimation();
				}
			}

			if (Mouse.current.leftButton.wasReleasedThisFrame && _isPressed)
			{
				_isPressed = false;
				PlayReleaseAnimation();

				if (IsMouseOverDice())
				{
					OnDiceClicked?.Invoke();
				}
			}
		}

		private bool IsMouseOverDice()
		{
			Vector2 mousePos = Mouse.current.position.ReadValue();
			Ray ray = _mainCamera.ScreenPointToRay(mousePos);

			if (Physics.Raycast(ray, out RaycastHit hit))
			{
				return hit.collider == diceCollider;
			}

			return false;
		}

		private void SetupVisual(string diceViewId)
		{
			foreach (var visual in _diceVisualMap.Values)
			{
				visual.gameObject.SetActive(false);
			}

			if (_diceVisualMap.TryGetValue(diceViewId, out var target))
			{
				target.gameObject.SetActive(true);
			}
		}

		public void SetRotation(int value)
		{
			var rotation = Vector3.zero;
			switch (value)
			{
				case 1:
					rotation = new Vector3(0, 0, 0);
					break;
				case 2:
					rotation = new Vector3(90, 0, 0);
					break;
				case 3:
					rotation = new Vector3(0, 0, -90);
					break;
				case 4:
					rotation = new Vector3(0, 0, 90);
					break;
				case 5:
					rotation = new Vector3(-90, 0, 0);
					break;
				case 6:
					rotation = new Vector3(-180, 0, 0);
					break;
			}

			model.localRotation = Quaternion.Euler(rotation);
		}

		public void UpdateChosenVisual(bool isChosen)
		{
			if (!outline.enabled)
			{
				outline.enabled = true;
			}

			if (isChosen)
			{
				model.DOLocalMove(Vector3.up * yOffset, animSpeed);
				outline.OutlineColor = Color.green;
			}
			else
			{
				model.DOLocalMove(Vector3.zero, animSpeed);
				outline.OutlineColor = Color.black;
			}
		}

		public void PlayPressAnimation()
		{
			model.transform.DOScale(0.9f, animSpeed);
		}

		public void PlayReleaseAnimation()
		{
			model.transform.DOScale(1f, animSpeed);
		}

		public void MoveToPosition(Vector3 position)
		{
			transform.DOMove(position, animSpeed);
		}
	}
}
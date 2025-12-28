using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

namespace _Main.Scripts.Dice
{
	public class DiceView : MonoBehaviour
	{
		[SerializeField] private MeshRenderer[] sideMeshes;
		[SerializeField] private Transform model;
		[SerializeField] private Outline outline;
		[SerializeField] private Collider diceCollider;

		[SerializeField] private float animSpeed = 0.15f;
		[SerializeField] private float yOffset = 0.02f;

		public event Action OnDiceClicked;

		private Camera _mainCamera;
		private bool _isPressed;

		public void Initialize()
		{
			_mainCamera = Camera.main;
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

		public void SetSideMesh(int value)
		{
			foreach (var mesh in sideMeshes)
			{
				mesh.enabled = false;
			}

			if (value > 0 && value <= sideMeshes.Length)
			{
				sideMeshes[value - 1].enabled = true;
			}
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
			transform.DOScale(0.9f, animSpeed);
		}

		public void PlayReleaseAnimation()
		{
			transform.DOScale(1f, animSpeed);
		}

		public void MoveToPosition(Vector3 position)
		{
			transform.DOMove(position, animSpeed);
		}
	}
}
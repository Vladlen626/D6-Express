using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace _Main.Scripts.Dice
{
	public class DiceView : MonoBehaviour
	{
		[HideInInspector]
		public UnityEvent OnDiceClicked;

		[HideInInspector]
		public UnityEvent OnDiceHoverEnter;

		[HideInInspector]
		public UnityEvent OnDiceHoverExit;

		[SerializeField] 
		private List<DiceVisualEntry> diceVisuals;
		
		[SerializeField] private Transform model;
		[SerializeField] private Outline outline;
		[SerializeField] private Collider diceCollider;

		[SerializeField] private float animSpeed = 0.3f;
		[SerializeField] private float yOffset = 0.02f;

		private Dictionary<string, Transform> _diceVisualMap;
		private Camera _mainCamera;
		private bool _isPressed;
		private bool isActive;
		private bool isPlayerDice;
		private bool _isHovered;

		public void Initialize(string diceConfigId, bool isPlayerDice)
		{
			_diceVisualMap = new Dictionary<string, Transform>();
			foreach (var entry in diceVisuals)
			{
				if (!_diceVisualMap.ContainsKey(entry.id))
				{
					_diceVisualMap.Add(entry.id, entry.visual);
				}
			}

			this.isPlayerDice = isPlayerDice;

			_mainCamera = Camera.main;
			isActive = true;
			SetupVisual(diceConfigId);
		}

		private void OnDestroy()
		{
			_isHovered = false;
			OnDiceHoverExit.Invoke();
			OnDiceClicked.RemoveAllListeners();
			OnDiceHoverEnter.RemoveAllListeners();
			OnDiceHoverExit.RemoveAllListeners();
		}

		private void Update()
		{
			if (!_mainCamera || !isPlayerDice)
			{
				return;
			}

			bool isMouseOver = IsMouseOverDice();

			if (isMouseOver && !_isHovered)
			{
				_isHovered = true;
				OnDiceHoverEnter?.Invoke();
			}

			if (!isMouseOver && _isHovered)
			{
				_isHovered = false;
				OnDiceHoverExit?.Invoke();
			}

			if (!isActive)
			{
				return;
			}

			if (Mouse.current.leftButton.wasPressedThisFrame)
			{
				if (isMouseOver)
				{
					_isPressed = true;
					PlayPressAnimation();
				}
			}

			if (Mouse.current.leftButton.wasReleasedThisFrame && _isPressed)
			{
				_isPressed = false;
				PlayReleaseAnimation();

				if (isMouseOver)
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

		public Tween MoveToPosition(Vector3 position, float speedMultiplier = 1)
		{
			return transform.DOMove(position, animSpeed * speedMultiplier);
		}

		public void Hide()
		{
			model.transform.DOScale(Vector3.zero, animSpeed/2);
			diceCollider.enabled = false;
			isActive = false;
		}

		public void Show()
		{
			model.transform.DOScale(Vector3.one, animSpeed/2);
			diceCollider.enabled = true;
			isActive = true;
		}

		// ReSharper disable Unity.PerformanceAnalysis
		public async UniTask PlayRollAnimationAsync(float rollTime = 2f)
		{
			var randomOffset = new Vector3(Random.Range(-0.02f, 0.02f), 0, Random.Range(-0.02f, 0.02f));

			var startPos = transform.position;
			var targetPos = startPos + randomOffset;

			float step = rollTime / 10f;
			float moveUpTime = step;
			float rotateTime = step * 8;
			float moveDownTime = step;

			var seq = DOTween.Sequence();

			seq.Append(transform.DOMove(targetPos + Vector3.up * 0.2f, moveUpTime).SetEase(Ease.Linear))
				.Join(transform.DORotate(Vector3.one * 180f, moveUpTime, RotateMode.FastBeyond360).SetEase(Ease.Linear))
				.Append(transform.DORotate(Vector3.one * (Random.Range(360f, 720f) * 5f), rotateTime, RotateMode.FastBeyond360)
					.SetEase(Ease.InOutQuad))
				.Append(transform.DOMove(targetPos, moveDownTime).SetEase(Ease.Linear))
				.Join(transform.DORotate(new Vector3(0f, Random.Range(0f, 360f), 0f), moveDownTime, RotateMode.FastBeyond360)
					.SetEase(Ease.Linear));

			await seq.ToUniTask();
		}
	}
}
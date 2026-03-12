using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Minimal 3D view for an item. Handles click detection and visibility.
	/// </summary>
	public class ItemView : MonoBehaviour
	{
		[SerializeField] private Collider clickCollider;
		[Header("State Animation")]
		[SerializeField] private float hoverLift = 0.025f;
		[SerializeField] private float hoverPunchDuration = 0.16f;
		[SerializeField] private float armedScaleMultiplier = 1.08f;
		[SerializeField] private float armedPulseDuration = 0.35f;
		[SerializeField] private float consumedScaleMultiplier = 0.55f;
		[SerializeField] private float consumedDuration = 0.35f;

		private IModifierItem boundItem;
		public UnityEvent OnClicked = new();
		public event Action<IModifierItem> OnHoverEnter;
		public event Action<IModifierItem> OnHoverExit;
		private Camera _cam;
		private bool _isHovered;
		private bool _isStateInitialized;
		private DiceItemState _currentState = DiceItemState.Ready;
		private Vector3 _baseScale;
		private Vector3 _baseLocalPosition;
		private Quaternion _baseLocalRotation;
		private Tween _hoverTween;
		private Tween _armedTween;
		private Tween _consumedTween;

		private void Awake()
		{
			_cam = Camera.main;
			_baseScale = transform.localScale;
			_baseLocalPosition = transform.localPosition;
			_baseLocalRotation = transform.localRotation;
		}

		public void Bind(IModifierItem item)
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
				UpdateState(boundItem.State, boundItem.IsVisible);
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

			boundItem.OnChanged -= OnItemChanged;
			boundItem = null;
			_isStateInitialized = false;
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

			KillTweens();
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
				PlayHoverAnimation();
			}
			else if (!isMouseOver && _isHovered)
			{
				_isHovered = false;
				OnHoverExit?.Invoke(boundItem);
				StopHoverTween();
				transform.DOLocalMove(_baseLocalPosition, 0.1f).SetEase(Ease.OutQuad);
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

			if (!isVisible)
			{
				KillTweens();
				gameObject.SetActive(false);
				_isStateInitialized = false;
				return;
			}

			if (!gameObject.activeSelf)
			{
				gameObject.SetActive(true);
			}

			if (!_isStateInitialized)
			{
				_currentState = state;
				_isStateInitialized = true;
				ApplyStateImmediate(state);
				return;
			}

			if (_currentState == state)
			{
				return;
			}

			ApplyStateTransition(state);
			_currentState = state;
		}

		public async UniTask WaitForConsumedAnimationAsync(int minDelayMs = 0)
		{
			if (_currentState != DiceItemState.Consumed)
			{
				return;
			}

			var animationTask = _consumedTween != null && _consumedTween.IsActive()
				? _consumedTween.AsyncWaitForCompletion().AsUniTask()
				: UniTask.CompletedTask;

			if (minDelayMs <= 0)
			{
				await animationTask;
				return;
			}

			await UniTask.WhenAll(animationTask, UniTask.Delay(minDelayMs));
		}

		private void ApplyStateImmediate(DiceItemState state)
		{
			KillTweens();

			switch (state)
			{
				case DiceItemState.Armed:
					transform.localScale = _baseScale;
					transform.localPosition = _baseLocalPosition;
					transform.localRotation = _baseLocalRotation;
					SetClickEnabled(true);
					StartArmedTween();
					break;

				case DiceItemState.Consumed:
					if (_isHovered && boundItem != null)
					{
						_isHovered = false;
						OnHoverExit?.Invoke(boundItem);
					}

					transform.localScale = ScaleBy(consumedScaleMultiplier);
					transform.localPosition = _baseLocalPosition;
					transform.localRotation = _baseLocalRotation;
					SetClickEnabled(false);
					break;

				default:
					transform.localScale = _baseScale;
					transform.localPosition = _baseLocalPosition;
					transform.localRotation = _baseLocalRotation;
					SetClickEnabled(state is DiceItemState.Ready or DiceItemState.Armed);
					break;
			}
		}

		private void ApplyStateTransition(DiceItemState newState)
		{
			switch (newState)
			{
				case DiceItemState.Armed:
					StopConsumedTween();
					transform.DOKill();
					transform.localPosition = _baseLocalPosition;
					transform.localRotation = _baseLocalRotation;
					SetClickEnabled(true);
					StartArmedTween();
					break;

				case DiceItemState.Consumed:
					if (_isHovered && boundItem != null)
					{
						_isHovered = false;
						OnHoverExit?.Invoke(boundItem);
					}

					StopHoverTween();
					StopArmedTween();
					SetClickEnabled(false);
					StartConsumedTween();
					break;

				default:
					StopHoverTween();
					StopArmedTween();
					StopConsumedTween();
					transform.DOKill();
					transform.DOScale(_baseScale, 0.12f).SetEase(Ease.OutQuad);
					transform.DOLocalMove(_baseLocalPosition, 0.12f).SetEase(Ease.OutQuad);
					transform.DOLocalRotateQuaternion(_baseLocalRotation, 0.12f).SetEase(Ease.OutQuad);
					SetClickEnabled(newState is DiceItemState.Ready or DiceItemState.Armed);
					break;
			}
		}

		private void StartArmedTween()
		{
			StopArmedTween();
			_armedTween = transform.DOScale(ScaleBy(armedScaleMultiplier), armedPulseDuration)
				.SetEase(Ease.InOutSine)
				.SetLoops(-1, LoopType.Yoyo);
		}

		private void StartConsumedTween()
		{
			StopConsumedTween();
			_consumedTween = DOTween.Sequence()
				.Append(transform.DOScale(ScaleBy(consumedScaleMultiplier), consumedDuration).SetEase(Ease.OutCubic))
				.Join(transform.DOLocalRotate(new Vector3(0f, 14f, 0f), consumedDuration, RotateMode.LocalAxisAdd).SetEase(Ease.OutQuad));
		}

		private void SetClickEnabled(bool enabled)
		{
			if (clickCollider)
			{
				clickCollider.enabled = enabled;
			}
		}

		private Vector3 ScaleBy(float multiplier)
		{
			return _baseScale * multiplier;
		}

		private void StopHoverTween()
		{
			if (_hoverTween != null && _hoverTween.IsActive())
			{
				_hoverTween.Kill();
			}

			_hoverTween = null;
		}

		private void StopArmedTween()
		{
			if (_armedTween != null && _armedTween.IsActive())
			{
				_armedTween.Kill();
			}

			_armedTween = null;
		}

		private void StopConsumedTween()
		{
			if (_consumedTween != null && _consumedTween.IsActive())
			{
				_consumedTween.Kill();
			}

			_consumedTween = null;
		}

		private void KillTweens()
		{
			StopHoverTween();
			StopArmedTween();
			StopConsumedTween();
			transform.DOKill();
		}

		private void PlayHoverAnimation()
		{
			if (_currentState == DiceItemState.Consumed)
			{
				return;
			}

			StopHoverTween();
			_hoverTween = transform.DOPunchPosition(Vector3.up * hoverLift, hoverPunchDuration, 1, 0f);
		}
	}
}

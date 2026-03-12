using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using _Main.Scripts.UI;
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
		[Header("References")]
		[SerializeField] private Transform visualRoot;
		[SerializeField] private Collider clickCollider;
		[SerializeField] private Outline visualOutline;

		[Header("State Animation")]
		[SerializeField] private float hoverLift = 0.025f;
		[SerializeField] private float hoverEnterDuration = 0.12f;
		[SerializeField] private float hoverExitDuration = 0.12f;
		[SerializeField] private Ease hoverEnterEase = Ease.OutQuad;
		[SerializeField] private Ease hoverExitEase = Ease.OutQuad;
		[SerializeField] private float armedScaleMultiplier = 1.08f;
		[SerializeField] private float armedPulseDuration = 0.35f;
		[SerializeField] private Ease armedPulseEase = Ease.InOutSine;
		[SerializeField] private float consumedScaleMultiplier = 0.55f;
		[SerializeField] private float consumedDuration = 0.35f;
		[SerializeField] private Vector3 consumedRotationOffset = new(0f, 14f, 0f);
		[SerializeField] private Ease consumedScaleEase = Ease.OutCubic;
		[SerializeField] private Ease consumedRotationEase = Ease.OutQuad;
		[SerializeField] private Ease consumedMoveEase = Ease.OutQuad;
		[SerializeField] private float settleDuration = 0.12f;
		[SerializeField] private Ease settleEase = Ease.OutQuad;

		[Header("Outline Colors")]
		[SerializeField] private ColorStyleRef hoverOutlineColor;
		[SerializeField] private ColorStyleRef armedOutlineColor;
		[SerializeField] private ColorStyleRef consumedOutlineColor;

		private IModifierItem boundItem;
		public UnityEvent OnClicked = new();
		public event Action<IModifierItem> OnHoverEnter;
		public event Action<IModifierItem> OnHoverExit;
		private Camera _cam;
		private bool _isHovered;
		private bool _isStateInitialized;
		private DiceItemState _currentState = DiceItemState.Ready;
		private Vector3 _baseVisualScale = Vector3.one;
		private Vector3 _baseVisualLocalPosition;
		private Quaternion _baseVisualLocalRotation;
		private Color _baseOutlineColor;
		private Tween _hoverMoveTween;
		private Tween _armedTween;
		private Tween _consumedTween;

		private void Awake()
		{
			_cam = Camera.main;

			_baseVisualScale = visualRoot.localScale;
			_baseVisualLocalPosition = visualRoot.localPosition;
			_baseVisualLocalRotation = visualRoot.localRotation;

			if (!visualOutline.enabled)
			{
				visualOutline.enabled = true;
			}

			_baseOutlineColor = visualOutline.OutlineColor;
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

		public void SetBaseLocalTransform(Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			transform.localPosition = localPosition;
			transform.localRotation = localRotation;
			transform.localScale = localScale;

			if (_isStateInitialized)
			{
				ApplyStateImmediate(_currentState);
				ApplyOutlineColor();
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
				StartHoverAnimation();
				ApplyOutlineColor();
			}
			else if (!isMouseOver && _isHovered)
			{
				_isHovered = false;
				OnHoverExit?.Invoke(boundItem);
				StopHoverAnimation();
				ApplyOutlineColor();
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
				ApplyOutlineColor();
				return;
			}

			if (_currentState == state)
			{
				ApplyOutlineColor();
				return;
			}

			ApplyStateTransition(state);
			_currentState = state;
			ApplyOutlineColor();
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
			var targetPosition = _isHovered ? GetHoverLocalPosition() : _baseVisualLocalPosition;

			switch (state)
			{
				case DiceItemState.Armed:
					visualRoot.localScale = _baseVisualScale;
					visualRoot.localPosition = targetPosition;
					visualRoot.localRotation = _baseVisualLocalRotation;
					SetClickEnabled(true);
					StartArmedTween();
					break;

				case DiceItemState.Consumed:
					if (_isHovered && boundItem != null)
					{
						_isHovered = false;
						OnHoverExit?.Invoke(boundItem);
					}

					visualRoot.localScale = ScaleBy(consumedScaleMultiplier);
					visualRoot.localPosition = _baseVisualLocalPosition;
					visualRoot.localRotation = _baseVisualLocalRotation;
					SetClickEnabled(false);
					break;

				default:
					visualRoot.localScale = _baseVisualScale;
					visualRoot.localPosition = targetPosition;
					visualRoot.localRotation = _baseVisualLocalRotation;
					SetClickEnabled(state is DiceItemState.Ready or DiceItemState.Armed);
					break;
			}
		}

		private void ApplyStateTransition(DiceItemState newState)
		{
			var targetPosition = _isHovered ? GetHoverLocalPosition() : _baseVisualLocalPosition;

			switch (newState)
			{
				case DiceItemState.Armed:
					StopConsumedTween();
					visualRoot.DOKill();
					visualRoot.localPosition = targetPosition;
					visualRoot.localRotation = _baseVisualLocalRotation;
					SetClickEnabled(true);
					StartArmedTween();
					break;

				case DiceItemState.Consumed:
					if (_isHovered && boundItem != null)
					{
						_isHovered = false;
						OnHoverExit?.Invoke(boundItem);
					}

					StopHoverMoveTween();
					StopArmedTween();
					SetClickEnabled(false);
					StartConsumedTween();
					break;

				default:
					StopHoverMoveTween();
					StopArmedTween();
					StopConsumedTween();
					visualRoot.DOKill();
					visualRoot.DOScale(_baseVisualScale, settleDuration).SetEase(settleEase);
					visualRoot.DOLocalMove(targetPosition, settleDuration).SetEase(settleEase);
					visualRoot.DOLocalRotateQuaternion(_baseVisualLocalRotation, settleDuration).SetEase(settleEase);
					SetClickEnabled(newState is DiceItemState.Ready or DiceItemState.Armed);
					break;
			}
		}

		private void StartArmedTween()
		{
			StopArmedTween();
			_armedTween = visualRoot.DOScale(ScaleBy(armedScaleMultiplier), armedPulseDuration)
				.SetEase(armedPulseEase)
				.SetLoops(-1, LoopType.Yoyo);
		}

		private void StartConsumedTween()
		{
			StopConsumedTween();
			_consumedTween = DOTween.Sequence()
				.Append(visualRoot.DOScale(ScaleBy(consumedScaleMultiplier), consumedDuration).SetEase(consumedScaleEase))
				.Join(visualRoot.DOLocalRotate(consumedRotationOffset, consumedDuration, RotateMode.LocalAxisAdd).SetEase(consumedRotationEase))
				.Join(visualRoot.DOLocalMove(_baseVisualLocalPosition, consumedDuration).SetEase(consumedMoveEase));
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
			return _baseVisualScale * multiplier;
		}

		private void StopHoverMoveTween()
		{
			if (_hoverMoveTween != null && _hoverMoveTween.IsActive())
			{
				_hoverMoveTween.Kill();
			}

			_hoverMoveTween = null;
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
			StopHoverMoveTween();
			StopArmedTween();
			StopConsumedTween();
			visualRoot.DOKill();
		}

		private void StartHoverAnimation()
		{
			if (_currentState == DiceItemState.Consumed)
			{
				return;
			}

			StopHoverMoveTween();
			_hoverMoveTween = visualRoot.DOLocalMove(GetHoverLocalPosition(), hoverEnterDuration).SetEase(hoverEnterEase);
		}

		private void StopHoverAnimation()
		{
			StopHoverMoveTween();
			_hoverMoveTween = visualRoot.DOLocalMove(_baseVisualLocalPosition, hoverExitDuration).SetEase(hoverExitEase);
		}

		private Vector3 GetHoverLocalPosition()
		{
			return _baseVisualLocalPosition + (Vector3.up * hoverLift);
		}

		private void ApplyOutlineColor()
		{
			if (!visualOutline.enabled)
			{
				visualOutline.enabled = true;
			}

			if (_currentState == DiceItemState.Consumed)
			{
				visualOutline.OutlineColor = consumedOutlineColor.Value;
				return;
			}

			if (_isHovered)
			{
				visualOutline.OutlineColor = hoverOutlineColor.Value;
				return;
			}

			if (_currentState == DiceItemState.Armed)
			{
				visualOutline.OutlineColor = armedOutlineColor.Value;
				return;
			}

			visualOutline.OutlineColor = _baseOutlineColor;
		}
	}
}

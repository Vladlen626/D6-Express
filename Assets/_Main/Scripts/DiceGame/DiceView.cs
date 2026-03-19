using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using PlatformCore.Services.Audio;
using UnityEngine.Events;
using _Main.Scripts.UI;
using Random = UnityEngine.Random;

namespace _Main.Scripts.Dice
{
	public class DiceView : MonoBehaviour
	{
		[HideInInspector]
		public UnityEvent OnDiceClicked = new UnityEvent();
		
		[HideInInspector]
		public UnityEvent OnDiceRelease = new UnityEvent();

		[HideInInspector]
		public UnityEvent OnDiceHoverEnter = new UnityEvent();

		[HideInInspector]
		public UnityEvent OnDiceHoverExit = new UnityEvent();

		[SerializeField] 
		private List<DiceVisualEntry> diceVisuals;
		
		[SerializeField] private Transform model;
		[SerializeField] private Outline outline;
		[SerializeField] private Collider diceCollider;

		[SerializeField] private float animSpeed = 0.10f;
		[SerializeField] private float yOffset = 0.02f;
		[SerializeField] private float itemTargetYOffset = 0.05f;
		[SerializeField] private float hoverScaleMultiplier = 1.06f;
		[SerializeField] private ColorStyleRef defaultOutlineColor;
		[SerializeField] private ColorStyleRef selectedOutlineColor;
		[SerializeField] private ColorStyleRef itemTargetSelectedOutlineColor;
		[SerializeField] private ColorStyleRef hoverOutlineColor;

		private IAudioService _audioService;
		private Dictionary<string, Transform> _diceVisualMap;
		private Camera _mainCamera;
		private bool _isPressed;
		private bool isActive;
		private bool isPlayerDice;
		private bool _isHovered;
		private bool isInAnimation;
		private bool isChosenVisual;
		private bool isItemTargetSelectedVisual;
		private float visualScale = 1f;
		private Vector3 baseModelScale = Vector3.one;
		private Tween moveTween;
		private Tween upgradeRotateTween;
		private Sequence upgradeStopSequence;

		public void Initialize(string diceConfigId, bool isPlayerDice, IAudioService audioService)
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
			_audioService = audioService;
			// ReSharper disable once Unity.PerformanceCriticalCodeCameraMain
			_mainCamera = Camera.main;
			isActive = true;
			baseModelScale = model ? model.localScale : Vector3.one;
			visualScale = 1f;
			isChosenVisual = false;
			isItemTargetSelectedVisual = false;
			SetupVisual(diceConfigId);
			ApplySelectionVisual();
		}

		private void OnDestroy()
		{
			KillMoveTween();
			KillUpgradeSpinSequence();
			_isHovered = false;
			OnDiceHoverExit?.Invoke();
			OnDiceClicked?.RemoveAllListeners();
			OnDiceRelease?.RemoveAllListeners();
			OnDiceHoverEnter?.RemoveAllListeners();
			OnDiceHoverExit?.RemoveAllListeners();
		}

		private void Update()
		{
			if (!_mainCamera || !isPlayerDice || isInAnimation || Mouse.current == null)
			{
				return;
			}

			bool isMouseOver = IsMouseOverDice();

			if (isMouseOver && !_isHovered)
			{
				_isHovered = true;
				OnDiceHoverEnter?.Invoke();
				ApplySelectionVisual();
			}

			if (!isMouseOver && _isHovered)
			{
				_isHovered = false;
				OnDiceHoverExit?.Invoke();
				ApplySelectionVisual();
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
					OnDiceRelease?.Invoke();
				}
			}

			if (Mouse.current.leftButton.wasReleasedThisFrame && _isPressed)
			{
				_isPressed = false;
				_audioService.PlaySoundAt(SoundNames.DiceClick, transform.position);
				PlayReleaseAnimation();

				if (isMouseOver)
				{
					OnDiceClicked?.Invoke();
				}
			}
		}

		private bool IsMouseOverDice()
		{
			if (Mouse.current == null)
			{
				return false;
			}

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
			if (!model)
			{
				return;
			}

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
			if (!model || !outline)
			{
				return;
			}

			isChosenVisual = isChosen;
			ApplySelectionVisual();
		}

		public void SetItemTargetSelectedVisual(bool isSelected)
		{
			if (!model || !outline)
			{
				return;
			}

			if (isItemTargetSelectedVisual == isSelected)
			{
				return;
			}

			isItemTargetSelectedVisual = isSelected;
			ApplySelectionVisual();
		}

		private void ApplySelectionVisual()
		{
			if (!model || !outline)
			{
				return;
			}

			if (!outline.enabled)
			{
				outline.enabled = true;
			}

			var targetY = 0f;
			var outlineColor = defaultOutlineColor.Value;
			var scaleMultiplier = _isHovered ? hoverScaleMultiplier : 1f;

			if (isItemTargetSelectedVisual)
			{
				targetY = itemTargetYOffset;
				outlineColor = itemTargetSelectedOutlineColor.Value;
			}
			else if (isChosenVisual)
			{
				targetY = yOffset;
				outlineColor = selectedOutlineColor.Value;
			}

			if (_isHovered)
			{
				outlineColor = hoverOutlineColor.Value;
			}

			model.DOLocalMove(Vector3.up * targetY, animSpeed);
			model.DOScale(GetScaledModelScale(scaleMultiplier), animSpeed);
			outline.OutlineColor = outlineColor;
		}

		public void PlayPressAnimation()
		{
			if (!model)
			{
				return;
			}

			model.transform.DOScale(GetScaledModelScale(GetCurrentVisualScaleMultiplier() * 0.9f), animSpeed);
		}

		public void PlayReleaseAnimation()
		{
			if (!model)
			{
				return;
			}

			model.transform.DOScale(GetScaledModelScale(GetCurrentVisualScaleMultiplier()), animSpeed);
		}

		public Tween MoveToPosition(Vector3 position, float speedMultiplier = 1)
		{
			KillMoveTween();
			_audioService?.PlaySoundAt(SoundNames.DiceMove, transform.position);
			moveTween = transform.DOMove(position, animSpeed / speedMultiplier);
			return moveTween;
		}

		public void ResetYRotation()
		{
			var rotation = transform.localRotation.eulerAngles;
			rotation.y = 0;
			transform.DOLocalRotate(rotation, animSpeed);
		}

		public void Hide()
		{
			KillMoveTween();
			KillUpgradeSpinSequence();
			if (!model)
			{
				isActive = false;
				return;
			}

			model.transform.DOKill();
			model.transform.localScale = Vector3.zero;
			if (diceCollider)
			{
				diceCollider.enabled = false;
			}
			isActive = false;
		}

		public void Show()
		{
			if (!model)
			{
				isActive = false;
				return;
			}

			model.transform.DOKill();
			model.transform.DOScale(GetScaledModelScale(), animSpeed / 2f);
			if (diceCollider)
			{
				diceCollider.enabled = true;
			}
			isActive = true;
		}

		public void StartUpgradeSpin()
		{
			if (!model)
			{
				return;
			}

			KillUpgradeSpinSequence();
			isInAnimation = true;

			const float rotateDuration = 0.45f;

			_audioService?.PlaySoundAt(SoundNames.DiceMove, transform.position);

			upgradeRotateTween = transform.DOLocalRotate(new Vector3(560f, 680f, 720f), rotateDuration, RotateMode.FastBeyond360)
				.SetRelative(true)
				.SetEase(Ease.Linear)
				.SetLoops(-1, LoopType.Restart);
		}

		public async UniTask StopUpgradeSpinAsync(int value)
		{
			if (!model)
			{
				return;
			}

			if (!isInAnimation)
			{
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
				SetRotation(value);
				return;
			}

			if (upgradeRotateTween != null && upgradeRotateTween.IsActive())
			{
				upgradeRotateTween.Kill();
			}

			upgradeRotateTween = null;

			const float settleDuration = 0.2f;
			var extraRotation = new Vector3(
				Random.Range(260f, 460f),
				Random.Range(340f, 640f),
				Random.Range(300f, 560f));

			upgradeStopSequence = DOTween.Sequence()
				.Append(transform.DOLocalRotate(extraRotation, settleDuration, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.OutQuad))
				.OnComplete(() =>
				{
					transform.localPosition = Vector3.zero;
					transform.localRotation = Quaternion.identity;
					SetRotation(value);
					isInAnimation = false;
					_audioService?.PlaySoundAt(SoundNames.DiceMove, transform.position);
				});

			await upgradeStopSequence.AsyncWaitForCompletion().AsUniTask();
		}

		// ReSharper disable Unity.PerformanceAnalysis
		public async UniTask PlayRollAnimationAsync(float rollTime = 2f)
		{
			if (!model)
			{
				return;
			}

			KillMoveTween();
			isInAnimation = true;
			var randomOffset = new Vector3(Random.Range(-0.02f, 0.02f), 0, Random.Range(-0.02f, 0.02f));

			var startPos = transform.position;
			var targetPos = startPos + randomOffset;

			float step = rollTime / 10f;
			float moveUpTime = step;
			float rotateTime = step * 8;
			float moveDownTime = step;

			var seq = DOTween.Sequence();

			_audioService?.PlaySoundAt(SoundNames.DiceMove, transform.position);
			seq.Append(transform.DOMove(targetPos + Vector3.up * 0.2f, moveUpTime).SetEase(Ease.Linear))
				.Join(transform.DORotate(Vector3.one * 180f, moveUpTime, RotateMode.FastBeyond360).SetEase(Ease.Linear))
				.Append(transform.DORotate(Vector3.one * (Random.Range(360f, 720f) * 5f), rotateTime, RotateMode.FastBeyond360)
					.SetEase(Ease.InOutQuad))
				.Append(transform.DOMove(targetPos, moveDownTime).SetEase(Ease.Linear)).OnComplete(() =>
				{
					_audioService?.PlaySoundAt(SoundNames.DiceMove, transform.position);
					isInAnimation = false;
				})
				.Join(transform.DORotate(new Vector3(0f, Random.Range(0f, 360f), 0f), moveDownTime, RotateMode.FastBeyond360)
					.SetEase(Ease.Linear));

			await seq.AsyncWaitForCompletion().AsUniTask();
		}

		private void KillUpgradeSpinSequence()
		{
			if (upgradeRotateTween != null && upgradeRotateTween.IsActive())
			{
				upgradeRotateTween.Kill();
			}

			if (upgradeStopSequence != null && upgradeStopSequence.IsActive())
			{
				upgradeStopSequence.Kill();
			}

			upgradeRotateTween = null;
			upgradeStopSequence = null;
			isInAnimation = false;
		}

		private void KillMoveTween()
		{
			if (moveTween != null && moveTween.IsActive())
			{
				moveTween.Kill();
			}

			moveTween = null;
		}

		private Vector3 GetScaledModelScale(float multiplier = 1f)
		{
			return baseModelScale * (visualScale * multiplier);
		}

		private float GetCurrentVisualScaleMultiplier()
		{
			return _isHovered ? hoverScaleMultiplier : 1f;
		}
	}
}

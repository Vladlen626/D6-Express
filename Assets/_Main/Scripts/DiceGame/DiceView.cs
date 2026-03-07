using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using PlatformCore.Services.Audio;
using UnityEngine.Events;
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

		private IAudioService _audioService;
		private Dictionary<string, Transform> _diceVisualMap;
		private Camera _mainCamera;
		private bool _isPressed;
		private bool isActive;
		private bool isPlayerDice;
		private bool _isHovered;
		private bool isInAnimation;
		private float visualScale = 1f;
		private Vector3 baseModelScale = Vector3.one;
		private Vector3 upgradeRollStartPosition = Vector3.zero;
		private Quaternion upgradeRollStartRotation = Quaternion.identity;
		private Tween upgradeHoverTween;
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
			SetupVisual(diceConfigId);
		}

		private void OnDestroy()
		{
			KillUpgradeSpinSequence();
			_isHovered = false;
			OnDiceHoverExit?.Invoke();
			OnDiceClicked?.RemoveAllListeners();
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
			if (!model)
			{
				return;
			}

			model.transform.DOScale(GetScaledModelScale(0.9f), animSpeed);
		}

		public void PlayReleaseAnimation()
		{
			if (!model)
			{
				return;
			}

			model.transform.DOScale(GetScaledModelScale(1f), animSpeed);
		}

		public Tween MoveToPosition(Vector3 position, float speedMultiplier = 1)
		{
			_audioService?.PlaySoundAt(SoundNames.DiceMove, transform.position);
			return transform.DOMove(position, animSpeed / speedMultiplier);
		}

		public void ResetYRotation()
		{
			var rotation = transform.localRotation.eulerAngles;
			rotation.y = 0;
			transform.DOLocalRotate(rotation, animSpeed);
		}

		public void Hide()
		{
			KillUpgradeSpinSequence();
			if (!model)
			{
				isActive = false;
				return;
			}

			transform.DOKill();
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

			transform.DOKill();
			model.transform.DOKill();
			model.transform.DOScale(GetScaledModelScale(), animSpeed / 2f);
			if (diceCollider)
			{
				diceCollider.enabled = true;
			}
			isActive = true;
		}

		public void SetVisualScale(float scale)
		{
			visualScale = Mathf.Max(0.01f, scale);
			if (isActive && model)
			{
				model.localScale = GetScaledModelScale();
			}
		}

		public void StartUpgradeSpin()
		{
			if (!model)
			{
				return;
			}

			KillUpgradeSpinSequence();
			upgradeRollStartPosition = transform.position;
			upgradeRollStartRotation = transform.rotation;
			isInAnimation = true;

			const float hoverHeight = 0.2f;
			const float hoverDuration = 0.24f;
			const float rotateDuration = 0.35f;

			var hoverTarget = upgradeRollStartPosition + Vector3.up * hoverHeight;

			_audioService?.PlaySoundAt(SoundNames.DiceMove, transform.position);

			upgradeHoverTween = transform.DOMoveY(hoverTarget.y, hoverDuration)
				.SetEase(Ease.InOutSine)
				.SetLoops(-1, LoopType.Yoyo);

			upgradeRotateTween = transform.DORotate(new Vector3(560f, 680f, 720f), rotateDuration, RotateMode.FastBeyond360)
				.SetRelative(true)
				.SetEase(Ease.Linear)
				.SetLoops(-1, LoopType.Restart);
		}

		public void StopUpgradeSpin(int value)
		{
			if (!model)
			{
				return;
			}

			if (!isInAnimation)
			{
				SetRotation(value);
				return;
			}

			if (upgradeHoverTween != null && upgradeHoverTween.IsActive())
			{
				upgradeHoverTween.Kill();
			}

			if (upgradeRotateTween != null && upgradeRotateTween.IsActive())
			{
				upgradeRotateTween.Kill();
			}

			upgradeHoverTween = null;
			upgradeRotateTween = null;

			const float dropDuration = 0.25f;
			var extraRotation = new Vector3(
				Random.Range(260f, 460f),
				Random.Range(340f, 640f),
				Random.Range(300f, 560f));

			upgradeStopSequence = DOTween.Sequence()
				.Append(transform.DOMove(upgradeRollStartPosition, dropDuration).SetEase(Ease.InQuad))
				.Join(transform.DORotate(extraRotation, dropDuration, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear))
				.OnComplete(() =>
				{
					transform.position = upgradeRollStartPosition;
					transform.rotation = upgradeRollStartRotation;
					SetRotation(value);
					isInAnimation = false;
					_audioService?.PlaySoundAt(SoundNames.DiceMove, transform.position);
				});
		}

		// ReSharper disable Unity.PerformanceAnalysis
		public async UniTask PlayRollAnimationAsync(float rollTime = 2f)
		{
			if (!model)
			{
				return;
			}

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

			await seq.AsyncWaitForCompletion().AsUniTask();;
		}

		private void KillUpgradeSpinSequence()
		{
			if (upgradeHoverTween != null && upgradeHoverTween.IsActive())
			{
				upgradeHoverTween.Kill();
			}

			if (upgradeRotateTween != null && upgradeRotateTween.IsActive())
			{
				upgradeRotateTween.Kill();
			}

			if (upgradeStopSequence != null && upgradeStopSequence.IsActive())
			{
				upgradeStopSequence.Kill();
			}

			transform.DOKill();
			if (model)
			{
				model.DOKill();
			}

			upgradeHoverTween = null;
			upgradeRotateTween = null;
			upgradeStopSequence = null;
			isInAnimation = false;
		}

		private Vector3 GetScaledModelScale(float multiplier = 1f)
		{
			return baseModelScale * (visualScale * multiplier);
		}
	}
}

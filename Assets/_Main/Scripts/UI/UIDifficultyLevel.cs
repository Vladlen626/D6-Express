using System;
using DG.Tweening;
using PlatformCore.Services.UI;
using UnityEngine;

namespace _Main.Scripts.UI
{
	public class UIDifficultyLevel : UIBaseElement
	{
		[SerializeField] private RectTransform easyButtonScaleTarget;
		[SerializeField] private RectTransform hardButtonScaleTarget;
		[SerializeField] private float hoverScaleMultiplier = 1.08f;
		[SerializeField] private float hoverScaleDuration = 0.12f;
		[SerializeField] private Ease hoverScaleEase = Ease.OutQuad;

		[SerializeField] private RectTransform hardButtonShakeTarget;
		[SerializeField] private float hardShakeDuration = 0.45f;
		[SerializeField] private Vector2 hardShakeStrength = new(2f, 1f);
		[SerializeField] private int hardShakeVibrato = 12;
		[SerializeField] private float hardShakeRandomness = 90f;

		public event Action OnEasyClicked;
		public event Action OnHardClicked;

		private Tween easyScaleTween;
		private Tween hardScaleTween;
		private Tween hardShakeTween;
		private Vector3 easyButtonBaseScale = Vector3.one;
		private Vector3 hardButtonBaseScale = Vector3.one;
		private Vector2 hardButtonBasePosition;

		protected override void OnAwake()
		{
			if (easyButtonScaleTarget)
			{
				easyButtonBaseScale = easyButtonScaleTarget.localScale;
			}

			if (hardButtonScaleTarget)
			{
				hardButtonBaseScale = hardButtonScaleTarget.localScale;
			}

			if (hardButtonShakeTarget)
			{
				hardButtonBasePosition = hardButtonShakeTarget.anchoredPosition;
			}
		}

		protected override void OnShow()
		{
			StartHardButtonShake();
		}

		protected override void OnHide()
		{
			StopHoverTweens();
			StopHardButtonShake();
		}

		private void OnDisable()
		{
			StopHoverTweens();
			StopHardButtonShake();
		}

		public void EasyBtn()
		{
			OnEasyClicked?.Invoke();
		}

		public void HardBtn()
		{
			OnHardClicked?.Invoke();
		}

		public void EasyHoverEnter()
		{
			AnimateHoverScale(easyButtonScaleTarget, ref easyScaleTween, easyButtonBaseScale * hoverScaleMultiplier);
		}

		public void EasyHoverExit()
		{
			AnimateHoverScale(easyButtonScaleTarget, ref easyScaleTween, easyButtonBaseScale);
		}

		public void HardHoverEnter()
		{
			AnimateHoverScale(hardButtonScaleTarget, ref hardScaleTween, hardButtonBaseScale * hoverScaleMultiplier);
		}

		public void HardHoverExit()
		{
			AnimateHoverScale(hardButtonScaleTarget, ref hardScaleTween, hardButtonBaseScale);
		}

		private void StartHardButtonShake()
		{
			StopHardButtonShake();

			if (!hardButtonShakeTarget)
			{
				return;
			}

			hardShakeTween = hardButtonShakeTarget
				.DOShakeAnchorPos(
					hardShakeDuration,
					hardShakeStrength,
					hardShakeVibrato,
					hardShakeRandomness,
					false,
					true)
				.SetLoops(-1, LoopType.Restart)
				.SetUpdate(true);
		}

		private void StopHardButtonShake()
		{
			if (hardShakeTween != null)
			{
				hardShakeTween.Kill();
				hardShakeTween = null;
			}

			if (!hardButtonShakeTarget)
			{
				return;
			}

			hardButtonShakeTarget.anchoredPosition = hardButtonBasePosition;
		}

		private void StopHoverTweens()
		{
			KillScaleTween(ref easyScaleTween);
			KillScaleTween(ref hardScaleTween);

			if (easyButtonScaleTarget)
			{
				easyButtonScaleTarget.localScale = easyButtonBaseScale;
			}

			if (hardButtonScaleTarget)
			{
				hardButtonScaleTarget.localScale = hardButtonBaseScale;
			}
		}

		private void AnimateHoverScale(RectTransform target, ref Tween tween, Vector3 targetScale)
		{
			if (!target)
			{
				return;
			}

			KillScaleTween(ref tween);

			tween = target
				.DOScale(targetScale, hoverScaleDuration)
				.SetEase(hoverScaleEase)
				.SetUpdate(true);
		}

		private static void KillScaleTween(ref Tween tween)
		{
			if (tween != null)
			{
				tween.Kill();
				tween = null;
			}
		}
	}
}

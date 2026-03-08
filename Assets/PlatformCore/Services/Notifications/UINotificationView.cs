using System;
using DG.Tweening;
using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Main.Scripts.UI;

public class UINotificationView : UIBaseElement
{
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private RectTransform contentRoot;
	[SerializeField] private UIBackgroundSizer backgroundSizer;
	[SerializeField] private Image backgroundImage;
	[SerializeField] private float initialShift = 50f;
	[SerializeField] private float smoothDuration = 0.5f;
	[SerializeField] private float fadeDuration = 0.3f;
	[SerializeField] private float showDelay = 2f;

	[SerializeField] private TextMeshProUGUI text;
	[SerializeField] private ColorStyleRef positiveColor;
	[SerializeField] private ColorStyleRef negativeColor;

	private Vector2 originalPos;
	private Sequence fullSequence;
	private RectTransform resolvedContentRoot;

	public event Action<UINotificationView> Showed;

	public void SetText(string text, bool isNegative = false)
	{
		this.text.text = text;
		ApplyToneColor(isNegative);
		if (backgroundSizer)
		{
			backgroundSizer.Refresh();
		}
	}

	private void ApplyToneColor(bool isNegative)
	{
		if (!backgroundImage)
		{
			throw new InvalidOperationException("Notification background image is not assigned.");
		}

		var style = isNegative ? negativeColor : positiveColor;
		if (string.IsNullOrWhiteSpace(style.Id))
		{
			var tone = isNegative ? "Negative" : "Positive";
			throw new InvalidOperationException($"{tone} notification color style is not assigned.");
		}

		backgroundImage.color = style.Value;
	}

	protected override void OnAwake()
	{
		base.OnAwake();

		if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
		if (contentRoot == null) contentRoot = GetComponent<RectTransform>();
		if (contentRoot == transform)
		{
			var child = transform.Find("ElementBackground");
			if (!child && transform.childCount > 0)
			{
				child = transform.GetChild(0);
			}

			if (child)
			{
				contentRoot = child as RectTransform;
			}
		}

		resolvedContentRoot = contentRoot != null && contentRoot != transform
			? contentRoot
			: null;

		if (resolvedContentRoot)
		{
			resolvedContentRoot.anchorMin = Vector2.zero;
			resolvedContentRoot.anchorMax = Vector2.one;
			resolvedContentRoot.pivot = new Vector2(0.5f, 0.5f);
			resolvedContentRoot.anchoredPosition = Vector2.zero;
			resolvedContentRoot.sizeDelta = Vector2.zero;
		}

		canvasGroup.alpha = 0f;
		text.gameObject.SetActive(false);
	}

	protected override void OnShow()
	{
		base.OnShow();

		RectTransform rectTransform = resolvedContentRoot ? resolvedContentRoot : contentRoot;
		var canMove = rectTransform != null && rectTransform != transform;

		if (canMove)
		{
			originalPos = rectTransform.anchoredPosition;

			rectTransform.anchoredPosition = originalPos + Vector2.down * initialShift;
		}
		canvasGroup.alpha = 0f;
		text.gameObject.SetActive(true);

		fullSequence?.Kill();
		fullSequence = DOTween.Sequence();

		if (canMove)
		{
			fullSequence.Append(
				rectTransform
					.DOAnchorPos(originalPos, smoothDuration)
					.SetEase(Ease.InOutQuad)
			);
		}

		fullSequence.Join(
			canvasGroup
				.DOFade(1f, fadeDuration)
				.SetEase(Ease.OutQuad)
		);

		fullSequence.AppendInterval(showDelay);

		if (canMove)
		{
			fullSequence.Append(
				rectTransform
					.DOAnchorPosX(originalPos.x + 50f, fadeDuration)
					.SetEase(Ease.InQuad)
			);
		}

		fullSequence.Join(
			canvasGroup
				.DOFade(0f, fadeDuration)
				.SetEase(Ease.InQuad)
		);

		fullSequence.OnComplete(() =>
		{
			text.gameObject.SetActive(false);
			Showed?.Invoke(this);
			base.OnHide();
		});
	}


	protected override void OnHide()
	{
		fullSequence?.Kill();
		base.OnHide();
	}
}

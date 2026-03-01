using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

public class UIGlobalNotificationView : UIBaseElement
{
	[SerializeField] private RectTransform container;
	[SerializeField] private TextMeshProUGUI messageText;
	[SerializeField] private float fadeInDuration = 0.18f;
	[SerializeField] private float fadeOutDuration = 0.18f;
	[SerializeField] private float scaleIn = 0.96f;
	[SerializeField] private float popScale = 1.04f;
	[SerializeField] private float settleDuration = 0.08f;
	[SerializeField] private float slideOffset = 18f;

	private Sequence sequence;

	public async UniTask PlayAsync(string message, float holdSeconds)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			return;
		}

		if (!messageText || !_group)
		{
			return;
		}

		messageText.text = message;
		Show();

		_group.alpha = 0f;
		_group.interactable = false;
		_group.blocksRaycasts = false;

		var rect = container ? container : GetComponent<RectTransform>();
		var originalPos = rect ? rect.anchoredPosition : Vector2.zero;
		if (rect)
		{
			rect.localScale = Vector3.one * scaleIn;
			rect.anchoredPosition = originalPos + Vector2.down * slideOffset;
		}

		sequence?.Kill();
		sequence = DOTween.Sequence();

		sequence.Append(_group.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad));
		if (rect)
		{
			sequence.Join(rect.DOScale(popScale, fadeInDuration).SetEase(Ease.OutBack));
			sequence.Join(rect.DOAnchorPos(originalPos, fadeInDuration).SetEase(Ease.OutQuad));
			sequence.Append(rect.DOScale(1f, settleDuration).SetEase(Ease.OutQuad));
		}

		sequence.AppendInterval(Mathf.Max(0.2f, holdSeconds));

		sequence.Append(_group.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad));
		if (rect)
		{
			sequence.Join(rect.DOScale(scaleIn, fadeOutDuration).SetEase(Ease.InQuad));
			sequence.Join(rect.DOAnchorPos(originalPos + Vector2.up * (slideOffset * 0.4f), fadeOutDuration).SetEase(Ease.InQuad));
		}

		await sequence.AsyncWaitForCompletion().AsUniTask();
		Hide();
	}

	public void Interrupt()
	{
		sequence?.Kill();
		Hide();
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		if (_group)
		{
			_group.interactable = false;
			_group.blocksRaycasts = false;
		}
	}

	protected override void OnHide()
	{
		sequence?.Kill();
		base.OnHide();
	}
}

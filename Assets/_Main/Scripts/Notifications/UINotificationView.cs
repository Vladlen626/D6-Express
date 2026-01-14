using System;
using DG.Tweening;
using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

public class UINotificationView : UIBaseElement
{
    [SerializeField] private float initialRightShift = 50f;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float smoothDuration = 0.5f;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float showDelay = 2f;

    [SerializeField] private TextMeshProUGUI text;

    private Vector2 originalPos;
    private Sequence fullSequence;

    public event Action<UINotificationView> Showed;

    public void SetText(string text)
    {
        this.text.text = text;
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        text.gameObject.SetActive(false);
    }

    protected override void OnShow()
    {
        base.OnShow();

        var rectTransform = text.GetComponent<RectTransform>();
        originalPos = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition += Vector2.right * initialRightShift;
        text.gameObject.SetActive(true);
        canvasGroup.alpha = 0f;

        fullSequence = DOTween.Sequence();

        fullSequence.Join(rectTransform.DOAnchorPosX(originalPos.x, smoothDuration).SetEase(Ease.InOutQuad));
        fullSequence.Join(canvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad));

        fullSequence.AppendInterval(showDelay);

        fullSequence.Join(rectTransform.DOAnchorPosX(originalPos.x + 50f, fadeDuration).SetEase(Ease.InQuad));
        fullSequence.Join(canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InQuad));

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

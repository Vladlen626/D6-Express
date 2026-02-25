using System;
using DG.Tweening;
using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

public class UINotificationView : UIBaseElement
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private float initialShift = 50f;
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
        if (contentRoot == null) contentRoot = GetComponent<RectTransform>();
        canvasGroup.alpha = 0f;
        text.gameObject.SetActive(false);
    }

    protected override void OnShow()
    {
        base.OnShow();

        RectTransform rectTransform = contentRoot;

        originalPos = rectTransform.anchoredPosition;

        rectTransform.anchoredPosition = originalPos + Vector2.down * initialShift;
        canvasGroup.alpha = 0f;
        text.gameObject.SetActive(true);

        fullSequence?.Kill();
        fullSequence = DOTween.Sequence();

        fullSequence.Append(
            rectTransform
                .DOAnchorPos(originalPos, smoothDuration)
                .SetEase(Ease.InOutQuad)
        );

        fullSequence.Join(
            canvasGroup
                .DOFade(1f, fadeDuration)
                .SetEase(Ease.OutQuad)
        );

        fullSequence.AppendInterval(showDelay);

        fullSequence.Append(
            rectTransform
                .DOAnchorPosX(originalPos.x + 50f, fadeDuration)
                .SetEase(Ease.InQuad)
        );

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

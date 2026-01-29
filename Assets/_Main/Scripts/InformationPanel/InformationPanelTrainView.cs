using TMPro;
using UnityEngine;
using DG.Tweening;

public class LedTrainView : MonoBehaviour
{
    [SerializeField] private LocalizedText informationText;
    [SerializeField] private RectTransform panel;
    [SerializeField] private float speed = 80f;

    private RectTransform _textRect;
    private Tween _tween;

    void Awake()
    {
        _textRect = informationText.GetComponent<RectTransform>();
        RestartMarquee();
    }

    void OnEnable() => RestartMarquee();

    void OnDisable()
    {
        _tween?.Kill();
        _tween = null;
    }

    public void SetText(string id, params string[] args)
    {
        informationText.SetText(id, args);
    }

    void RestartMarquee()
    {
        if (!isActiveAndEnabled || informationText == null || panel == null) return;

        _tween?.Kill();

        informationText.Tmp.ForceMeshUpdate();

        float panelLeft = panel.anchoredPosition.x;
        float panelRight = panelLeft + panel.rect.width;

        float textWidth = informationText.Tmp.preferredWidth;

        float startX = panelRight;

        float endX = panelLeft - textWidth;

        _textRect.anchoredPosition = new Vector2(startX, _textRect.anchoredPosition.y);

        float distance = Mathf.Abs(endX - startX);
        float duration = (speed <= 0f) ? 0.01f : distance / speed;

        _tween = _textRect
            .DOAnchorPosX(endX, duration, snapping: false)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart)
            .SetUpdate(true);
    }
}

using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlatformCore.Services.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIModifiersView : UIBaseElement, IBeginDragHandler, IEndDragHandler
{
    [SerializeField]
    private Transform list;

    [SerializeField]
    private LocalizedText header;

    [SerializeField]
    private CanvasGroup modifiersGroup;

    [SerializeField]
    private ScrollRect scrollRect;

    [SerializeField]
    private RectTransform viewport;

    [SerializeField]
    [Min(1)]
    private int maxVisibleItems = 3;

    [SerializeField]
    [Min(0.01f)]
    private float snapDuration = 0.2f;

    [SerializeField]
    [Min(1f)]
    private float snapVelocityThreshold = 40f;

    public Transform List => list;
    public LocalizedText Header => header;

    private bool isDragging;
    private bool pendingSnap;
    private bool isSnapping;
    private Tween snapTween;

    private void OnEnable()
    {
        if (!scrollRect)
        {
            return;
        }

        modifiersGroup.blocksRaycasts = true;

        scrollRect.onValueChanged.AddListener(OnScrollChanged);
    }

    private void OnDisable()
    {
        if (scrollRect)
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
        }

        StopSnapTween();
        isDragging = false;
        pendingSnap = false;
        isSnapping = false;
    }

    private void Update()
    {
        if (!pendingSnap || isDragging || isSnapping || !scrollRect || list is not RectTransform content)
        {
            return;
        }

        var axis = GetAxis(content);
        var velocity = axis == RectTransform.Axis.Vertical ? scrollRect.velocity.y : scrollRect.velocity.x;
        if (Mathf.Abs(velocity) > snapVelocityThreshold)
        {
            return;
        }

        SnapToNearest(content, axis);
    }

    public void RefreshVisibleWindow()
    {
        if (!scrollRect || !viewport || list is not RectTransform content)
        {
            return;
        }

        if (maxVisibleItems < 1)
        {
            return;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        var axis = GetAxis(content);
        var itemSize = GetItemSize(content, axis);
        if (itemSize <= 0f)
        {
            return;
        }

        var spacing = 0f;
        var padding = 0f;
        if (axis == RectTransform.Axis.Vertical && content.TryGetComponent<VerticalLayoutGroup>(out var verticalLayout))
        {
            spacing = verticalLayout.spacing;
            padding = verticalLayout.padding.top + verticalLayout.padding.bottom;
        }
        else if (axis == RectTransform.Axis.Horizontal && content.TryGetComponent<HorizontalLayoutGroup>(out var horizontalLayout))
        {
            spacing = horizontalLayout.spacing;
            padding = horizontalLayout.padding.left + horizontalLayout.padding.right;
        }

        var targetSize = (itemSize * maxVisibleItems) + (spacing * (maxVisibleItems - 1)) + padding;
        viewport.SetSizeWithCurrentAnchors(axis, targetSize);
        LayoutRebuilder.ForceRebuildLayoutImmediate(viewport);

        pendingSnap = true;
    }

    public async UniTask ShowModifiers()
    {
        RefreshVisibleWindow();

        await modifiersGroup
            .DOFade(1f, 0.5f)
            .SetEase(Ease.OutQuad)
            .AsyncWaitForCompletion();
        modifiersGroup.interactable = true;
    }

    public async UniTask HideModifiers()
    {
        await modifiersGroup
            .DOFade(0f, 0.5f)
            .SetEase(Ease.OutQuad)
            .AsyncWaitForCompletion();
        modifiersGroup.interactable = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        pendingSnap = true;
        StopSnapTween();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        pendingSnap = true;
    }

    private RectTransform.Axis GetAxis(RectTransform content)
    {
        if (scrollRect.horizontal && !scrollRect.vertical)
        {
            return RectTransform.Axis.Horizontal;
        }

        if (scrollRect.vertical && !scrollRect.horizontal)
        {
            return RectTransform.Axis.Vertical;
        }

        if (content.TryGetComponent<HorizontalLayoutGroup>(out _))
        {
            return RectTransform.Axis.Horizontal;
        }

        return RectTransform.Axis.Vertical;
    }

    private static float GetItemSize(RectTransform content, RectTransform.Axis axis)
    {
        for (var i = 0; i < content.childCount; i++)
        {
            if (content.GetChild(i) is not RectTransform child || !child.gameObject.activeInHierarchy)
            {
                continue;
            }

            var preferred = axis == RectTransform.Axis.Vertical
                ? LayoutUtility.GetPreferredHeight(child)
                : LayoutUtility.GetPreferredWidth(child);
            if (preferred > 0f)
            {
                return preferred;
            }

            var rectSize = axis == RectTransform.Axis.Vertical ? child.rect.height : child.rect.width;
            if (rectSize > 0f)
            {
                return rectSize;
            }
        }

        return 0f;
    }

    private void OnScrollChanged(Vector2 _)
    {
        if (isSnapping)
        {
            return;
        }

        pendingSnap = true;
    }

    private void SnapToNearest(RectTransform content, RectTransform.Axis axis)
    {
        if (!TryGetTargetNormalized(content, axis, out var targetNormalized))
        {
            pendingSnap = false;
            return;
        }

        var current = axis == RectTransform.Axis.Vertical
            ? scrollRect.verticalNormalizedPosition
            : scrollRect.horizontalNormalizedPosition;

        if (Mathf.Abs(current - targetNormalized) < 0.0001f)
        {
            pendingSnap = false;
            return;
        }

        StopSnapTween();
        isSnapping = true;

        snapTween = DOTween
            .To(
                () => axis == RectTransform.Axis.Vertical ? scrollRect.verticalNormalizedPosition : scrollRect.horizontalNormalizedPosition,
                value =>
                {
                    if (axis == RectTransform.Axis.Vertical)
                    {
                        scrollRect.verticalNormalizedPosition = value;
                    }
                    else
                    {
                        scrollRect.horizontalNormalizedPosition = value;
                    }
                },
                targetNormalized,
                snapDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                isSnapping = false;
                pendingSnap = false;
                snapTween = null;
            })
            .OnKill(() =>
            {
                isSnapping = false;
                snapTween = null;
            });
    }

    private bool TryGetTargetNormalized(RectTransform content, RectTransform.Axis axis, out float targetNormalized)
    {
        targetNormalized = 0f;

        var itemSize = GetItemSize(content, axis);
        if (itemSize <= 0f)
        {
            return false;
        }

        var spacing = 0f;
        if (axis == RectTransform.Axis.Vertical && content.TryGetComponent<VerticalLayoutGroup>(out var verticalLayout))
        {
            spacing = verticalLayout.spacing;
        }
        else if (axis == RectTransform.Axis.Horizontal && content.TryGetComponent<HorizontalLayoutGroup>(out var horizontalLayout))
        {
            spacing = horizontalLayout.spacing;
        }

        var step = itemSize + spacing;
        if (step <= 0f)
        {
            return false;
        }

        var contentSize = axis == RectTransform.Axis.Vertical ? content.rect.height : content.rect.width;
        var viewportSize = axis == RectTransform.Axis.Vertical ? viewport.rect.height : viewport.rect.width;
        var maxOffset = contentSize - viewportSize;
        if (maxOffset <= 0f)
        {
            return false;
        }

        var currentNormalized = axis == RectTransform.Axis.Vertical
            ? scrollRect.verticalNormalizedPosition
            : scrollRect.horizontalNormalizedPosition;

        var currentOffset = axis == RectTransform.Axis.Vertical
            ? (1f - currentNormalized) * maxOffset
            : currentNormalized * maxOffset;

        var targetOffset = Mathf.Clamp(Mathf.Round(currentOffset / step) * step, 0f, maxOffset);
        targetNormalized = axis == RectTransform.Axis.Vertical
            ? 1f - (targetOffset / maxOffset)
            : targetOffset / maxOffset;

        return true;
    }

    private void StopSnapTween()
    {
        if (snapTween != null && snapTween.IsActive())
        {
            snapTween.Kill();
        }
    }
}

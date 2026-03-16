using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CustomScrollList : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform _itemContainer;
    [SerializeField] private Button _arrowPrev;
    [SerializeField] private Button _arrowNext;

    [Header("Settings")]
    [SerializeField] private bool _isHorizontal = false;
    [SerializeField] private int _visibleCount = 3;
    [SerializeField] private float _itemSize = 120f;
    [SerializeField] private float _itemSpacing = 10f;

    [Header("Animation")]
    [SerializeField] private float _animDuration = 0.25f;
    [SerializeField] private Vector3 _hiddenScale = new Vector3(0.6f, 0.6f, 1f);
    [SerializeField] private float _hiddenAlpha = 0f;

    [Header("Input")]
    [SerializeField] private Key _keyNext = Key.DownArrow;
    [SerializeField] private Key _keyPrev = Key.UpArrow;

    private List<RectTransform> _items = new();
    private List<CanvasGroup> _canvasGroups = new();
    private int _startIndex = 0;
    private bool _isAnimating = false;
    private bool _initialized = false;
    private int _lastChildCount = -1;
    private Sequence _scrollSequence;

    private void Awake()
    {
        _arrowPrev.onClick.AddListener(() => Scroll(-1));
        _arrowNext.onClick.AddListener(() => Scroll(1));
        _arrowPrev.interactable = false;
        _arrowNext.interactable = false;
    }

    private void Update()
    {
        int currentCount = _itemContainer.childCount;
        if (currentCount != _lastChildCount)
        {
            _lastChildCount = currentCount;
            RebuildItemList();
        }

        if (!_initialized)
        {
            return;
        }

        if (Keyboard.current[_keyNext].wasPressedThisFrame)
        {
            Scroll(1);
        }

        if (Keyboard.current[_keyPrev].wasPressedThisFrame)
        {
            Scroll(-1);
        }
    }

    private void RebuildItemList()
    {
        KillActiveTweens();
        _isAnimating = false;

        _startIndex = Mathf.Clamp(
            _startIndex,
            0,
            Mathf.Max(0, _itemContainer.childCount - _visibleCount)
        );

        _items.Clear();
        _canvasGroups.Clear();

        for (int i = 0; i < _itemContainer.childCount; i++)
        {
            var rt = _itemContainer.GetChild(i) as RectTransform;
            _items.Add(rt);

            var cg = rt.GetComponent<CanvasGroup>();
            if (!cg)
            {
                cg = rt.gameObject.AddComponent<CanvasGroup>();
            }
            _canvasGroups.Add(cg);
        }

        _initialized = _items.Count > 0;
        RefreshImmediate();
    }

    public void Scroll(int direction)
    {
        if (_isAnimating || !_initialized)
        {
            return;
        }

        int newIndex = Mathf.Clamp(
            _startIndex + direction,
            0,
            Mathf.Max(0, _items.Count - _visibleCount)
        );

        if (newIndex == _startIndex)
        {
            return;
        }

        int oldIndex = _startIndex;
        _startIndex = newIndex;

        AnimateScroll(oldIndex, newIndex);
        UpdateArrows();
    }

    private void AnimateScroll(int oldStart, int newStart)
    {
        _isAnimating = true;
        int oldEnd = Mathf.Min(oldStart + _visibleCount, _items.Count);
        int newEnd = Mathf.Min(newStart + _visibleCount, _items.Count);

        KillActiveTweens();
        _scrollSequence = DOTween.Sequence();

        for (int i = oldStart; i < oldEnd; i++)
        {
            if (i < newStart || i >= newEnd)
            {
                _scrollSequence.Join(AnimateItem(i, _hiddenScale, _hiddenAlpha));
            }
        }

        for (int i = newStart; i < newEnd; i++)
        {
            if (i < oldStart || i >= oldEnd)
            {
                SetItemImmediate(i, _hiddenScale, _hiddenAlpha);
                _scrollSequence.Join(AnimateItem(i, Vector3.one, 1f));
            }
        }

        RepositionItems();

        _scrollSequence.OnComplete(() =>
        {
            _isAnimating = false;
            _scrollSequence = null;
        });
        _scrollSequence.Play();
    }

    private Tween AnimateItem(int index, Vector3 targetScale, float targetAlpha)
    {
        var rt = _items[index];
        var cg = _canvasGroups[index];
        var tween = DOTween.Sequence()
            .Join(rt.DOScale(targetScale, _animDuration))
            .Join(cg.DOFade(targetAlpha, _animDuration))
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                cg.blocksRaycasts = targetAlpha > 0f;
            });
        return tween;
    }

    private void RepositionItems()
    {
        float step = _itemSize + _itemSpacing;
        float offset = GetGroupOffset();

        for (int i = 0; i < _items.Count; i++)
        {
            int visiblePos = i - _startIndex;
            bool isVisible = visiblePos >= 0 && visiblePos < _visibleCount;

            float pos = visiblePos * step;
            _items[i].anchoredPosition = _isHorizontal
                ? new Vector2(offset + pos, 0)
                : new Vector2(0, -(offset + pos));

            if (!isVisible)
            {
                _items[i].localScale = _hiddenScale;
                _canvasGroups[i].alpha = _hiddenAlpha;
                _canvasGroups[i].blocksRaycasts = false;
            }
        }
    }

    private float GetGroupOffset()
    {
        int count = Mathf.Min(_visibleCount, _items.Count);
        float totalSize = count * _itemSize + (count - 1) * _itemSpacing;

        if (_isHorizontal)
        {
            return -totalSize / 2f + _itemSize / 2f;
        }

        return totalSize / 2f - _itemSize / 2f;
    }

    private void RefreshImmediate()
    {
        float step = _itemSize + _itemSpacing;
        float offset = GetGroupOffset();

        for (int i = 0; i < _items.Count; i++)
        {
            int visiblePos = i - _startIndex;
            bool isVisible = visiblePos >= 0 && visiblePos < _visibleCount;

            float pos = visiblePos * step;
            _items[i].anchoredPosition = _isHorizontal
                ? new Vector2(offset + pos, 0)
                : new Vector2(0, -(offset + pos));

            SetItemImmediate(
                i,
                isVisible ? Vector3.one : _hiddenScale,
                isVisible ? 1f : _hiddenAlpha
            );

            _canvasGroups[i].blocksRaycasts = isVisible;
        }

        UpdateArrows();
    }

    private void SetItemImmediate(int index, Vector3 scale, float alpha)
    {
        _items[index].localScale = scale;
        _canvasGroups[index].alpha = alpha;
    }

    private void UpdateArrows()
    {
        if (!_initialized)
        {
            _arrowPrev.interactable = false;
            _arrowNext.interactable = false;
            return;
        }

        _arrowPrev.interactable = _startIndex > 0;
        _arrowNext.interactable = _startIndex < _items.Count - _visibleCount;
    }

    private void KillActiveTweens()
    {
        if (_scrollSequence != null && _scrollSequence.IsActive())
        {
            _scrollSequence.Kill();
            _scrollSequence = null;
        }

        foreach (var item in _items)
        {
            item.DOKill();
        }

        foreach (var cg in _canvasGroups)
        {
            cg.DOKill();
        }
    }

    private void OnDestroy()
    {
        KillActiveTweens();
    }
}
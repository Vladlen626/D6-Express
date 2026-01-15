using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlatformCore.Services.UI;
using UnityEngine;

public class UITransitionView : UIBaseElement
{
    [SerializeField]
    private RectTransform upper;

    [SerializeField]
    private RectTransform bottom;

    [SerializeField]
    private AnimationCurve curve;

    private float initialUpperY, initialBottomY;

    private void Start()
    {
        initialUpperY = upper.anchoredPosition.y;
        initialBottomY = bottom.anchoredPosition.y;
    }

    public async Task ShowAsync(float duration)
    {
        Show();

        var upperMove = upper.DOAnchorPosY(0f, duration).SetEase(curve).AsyncWaitForCompletion().AsUniTask();
        var bottomMove = bottom.DOAnchorPosY(0f, duration).SetEase(curve).AsyncWaitForCompletion().AsUniTask();

        await UniTask.WhenAll(upperMove, bottomMove);
    }

    public async Task HideAsync(float duration)
    {
        var upperMove = upper.DOAnchorPosY(initialUpperY, duration).SetEase(curve).AsyncWaitForCompletion().AsUniTask();
        var bottomMove = bottom.DOAnchorPosY(initialBottomY, duration).SetEase(curve).AsyncWaitForCompletion().AsUniTask();

        await UniTask.WhenAll(upperMove, bottomMove);

        Hide();
    }

    protected override void OnHide()
    {
        base.OnHide();
    }

    protected override void OnShow()
    {
        base.OnShow();
    }
}
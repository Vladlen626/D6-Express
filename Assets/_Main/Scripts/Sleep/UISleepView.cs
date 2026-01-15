using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlatformCore.Services.UI;
using UnityEngine;

public class UISleepView : UIBaseElement
{
    [SerializeField]
    private RectTransform upper;

    [SerializeField]
    private RectTransform bottom;

    [SerializeField]
    private float duration;

    [SerializeField]
    private AnimationCurve curve;

    private float initialUpperY, initialBottomY;

    private void Start()
    {
        initialUpperY = upper.anchoredPosition.y;
        initialBottomY = bottom.anchoredPosition.y;
    }

    public UniTask CloseEyes()
    {
        var upperMove = upper.DOAnchorPosY(0f, duration).SetEase(curve).AsyncWaitForCompletion().AsUniTask();
        var bottomMove = bottom.DOAnchorPosY(0f, duration).SetEase(curve).AsyncWaitForCompletion().AsUniTask();

        return UniTask.WhenAll(upperMove, bottomMove);
    }

    public UniTask OpenEyes()
    {
        var upperMove = upper.DOAnchorPosY(initialUpperY, duration).SetEase(curve).AsyncWaitForCompletion().AsUniTask();
        var bottomMove = bottom.DOAnchorPosY(initialBottomY, duration).SetEase(curve).AsyncWaitForCompletion().AsUniTask();

        return UniTask.WhenAll(upperMove, bottomMove);
    }
}

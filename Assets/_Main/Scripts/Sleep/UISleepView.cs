using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlatformCore.Services.UI;
using UnityEngine;

public class UISleepView : UIBaseElement
{
    [SerializeField]
    private CanvasGroup wakeUpCanvasGroup;

    public async UniTask ShowWakeUp()
    {
        await wakeUpCanvasGroup
            .DOFade(1f, 0.5f)
            .SetEase(Ease.OutQuad);
    }

    public async UniTask HideWakeUp()
    {
        await wakeUpCanvasGroup
            .DOFade(0f, 0.5f)
            .SetEase(Ease.OutQuad);
    }
}

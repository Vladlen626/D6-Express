using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlatformCore.Services.UI;
using UnityEngine;

public class UIModifiersView : UIBaseElement
{
    [SerializeField]
    private Transform list;

    [SerializeField]
    private LocalizedText header;

    public Transform List => list;
    public LocalizedText Header => header;

    public async UniTask ShowModifiers()
    {
        await _group
            .DOFade(1f, 0.5f)
            .SetEase(Ease.OutQuad)
            .AsyncWaitForCompletion();
        _group.interactable = true;
    }

    public async UniTask HideModifiers()
    {
        await _group
            .DOFade(0f, 0.5f)
            .SetEase(Ease.OutQuad)
            .AsyncWaitForCompletion();
        _group.interactable = false;
    }
}

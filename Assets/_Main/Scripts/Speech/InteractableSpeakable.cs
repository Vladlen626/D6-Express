using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class InteractableSpeakable : Interactable
{
    [SerializeField] private int id = -1;

    private Quaternion lastRot;
    private Tween rotTween;

    public int Id => id;

    public override bool CanInteract(Interactor interactor) => id != -1;

    public override async void StartInteract(Interactor interactor)
    {
        lastRot = transform.rotation;

        rotTween?.Kill();

        rotTween = transform
            .DOLookAt(interactor.transform.position, 0.25f, AxisConstraint.Y) // rotate to face target position [web:1]
            .SetUpdate(true);

        await rotTween.ToUniTask(); // await tween completion [web:1]
    }

    public override async void StopInteract(Interactor interactor)
    {
        rotTween?.Kill();

        rotTween = transform
            .DORotateQuaternion(lastRot, 0.25f) // rotate back to saved rotation [web:1]
            .SetUpdate(true);

        await rotTween.ToUniTask(); // await tween completion [web:1]
    }
}

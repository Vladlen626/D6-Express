using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class InteractableActionSpeak : InteractionAction
{
	private CharacterStateController stateController;

    public int Id { get; private set; } = -1;

    public override void Init(Interactor interactor)
    {
        base.Init(interactor);

        stateController = interactor.GetComponent<CharacterStateController>();
    }

    public override bool CanInteract(IInteractable interactable)
    {
        return interactable is InteractableSpeakable;
    }

    protected override async void StartInteractInternal(IInteractable interactable)
    {
        base.StartInteractInternal(interactable);

        // todo: выглядит сомнительно
        var speakable = interactable as InteractableSpeakable;
        Id = speakable.Id;

        var targetable = speakable.GetComponent<Targetable>();

        var rotateTarget = targetable == null ? speakable.transform : targetable.CameraTarget;

        await Interactor.transform.DOLookAt(rotateTarget.position, 1).ToUniTask();

        stateController.TryEnterState(CharacterState.SPEAKING);
    }

    protected override async void StopInteractInternal(IInteractable interactable)
    {
        Id = -1;

        await Interactor.transform.DORotateQuaternion(Quaternion.identity, 1).ToUniTask();

        stateController.TryEnterState(CharacterState.DEFAULT);

        base.StopInteractInternal(interactable);
    }
}
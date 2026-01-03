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
        return interactable is InteractableSpeakable && stateController.HasState(CharacterState.DEFAULT);
    }

    protected override async void StartInteractInternal(IInteractable interactable)
    {
        base.StartInteractInternal(interactable);

        // todo: выглядит сомнительно
        var speakable = interactable as InteractableSpeakable;
        Id = speakable.Id;

        var targetable = speakable.GetComponent<Targetable>();

        var rotateTarget = targetable == null ? speakable.transform : targetable.CameraTarget;

        stateController.TryAddState(CharacterState.SPEAKING);

        var playerView = Interactor.GetComponent<PlayerView>();
        await playerView.CameraRoot.DOLookAt(rotateTarget.position, 1).ToUniTask();
    }

    protected override async void StopInteractInternal(IInteractable interactable)
    {
        Id = -1;

        stateController.TryRemoveState(CharacterState.SPEAKING);
        stateController.TryAddState(CharacterState.DEFAULT);

        base.StopInteractInternal(interactable);
    }
}
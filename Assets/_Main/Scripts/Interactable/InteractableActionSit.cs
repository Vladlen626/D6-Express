using System;
using System.Threading.Tasks;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlatformCore.Core;
using UnityEngine;

[Serializable]
public class InteractableActionSit : InteractionAction
{
    private CharacterStateController stateController;
    private Vector3 lastPos;

    public override void Init(Interactor interactor)
    {
        base.Init(interactor);

        stateController = interactor.GetComponent<CharacterStateController>();
    }

    public override bool CanInteract(IInteractable interactable)
    {
        return interactable is InteractableSittable && stateController.State == CharacterState.DEFAULT;
    }

    protected override async void StartInteractInternal(IInteractable interactable)
    {
        Interactor.GetComponent<CharacterStateController>().TryEnterState(CharacterState.TRANSITION);

        lastPos = Interactor.transform.position;

        var sittable = interactable as InteractableSittable;

        var moveTask = Interactor.transform.DOMove(sittable.SitTfm.position, 1).ToUniTask();
        var rotateTask = Interactor.transform.DORotateQuaternion(sittable.SitTfm.rotation, 1).ToUniTask();

        await UniTask.WhenAll(moveTask, rotateTask);

        Interactor.GetComponent<CharacterStateController>().TryEnterState(CharacterState.SITTING);

        Locator.Resolve<IInputService>().OnMoved += OnMoved;
    }

    protected override async void StopInteractInternal(IInteractable interactable)
    {
        Locator.Resolve<IInputService>().OnMoved -= OnMoved;

        await Interactor.transform.DOMove(lastPos, 0.25f).ToUniTask();

        Interactor.GetComponent<CharacterStateController>().TryEnterState(CharacterState.DEFAULT);
    }

    private void OnMoved(Vector2 dir)
    {
        StopInteract(null);
    }
}
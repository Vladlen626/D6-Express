using System;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlatformCore.Core;
using UnityEngine;

[Serializable]
public class InteractableActionLay : InteractionAction
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
        return interactable is InteractableLayable && stateController.State == CharacterState.DEFAULT;
    }

    protected override async void StartInteractInternal(IInteractable interactable)
    {
        stateController.TryEnterState(CharacterState.TRANSITION);

        lastPos = Interactor.transform.position;

        var layable = interactable as InteractableLayable;

        var moveTask = Interactor.transform.DOMove(layable.SitTfm.position, 1).ToUniTask();
        var rotateTask = Interactor.transform.DORotateQuaternion(layable.SitTfm.rotation, 1).ToUniTask();

        await UniTask.WhenAll(moveTask, rotateTask);

        stateController.TryEnterState(CharacterState.LAYING);
        inputService.OnMoved += OnMoved;
    }

    protected async override void StopInteractInternal(IInteractable interactable)
    {
        inputService.OnMoved -= OnMoved;

        var moveTask = Interactor.transform.DOMove(lastPos, 0.25f).ToUniTask();
        var rotateTask = Interactor.transform.DORotateQuaternion(Quaternion.identity, 0.25f).ToUniTask();

        await UniTask.WhenAll(moveTask, rotateTask);

        Interactor.GetComponent<CharacterStateController>().TryEnterState(CharacterState.DEFAULT);
    }

    private void OnMoved(Vector2 dir)
    {
        StopInteract(null);
    }
}
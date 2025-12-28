using System;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using UnityEngine;

[Serializable]
public class InteractableActionLay : InteractionAction
{
    private Vector3 lastPos;

    public override bool CanInteract(IInteractable interactable)
    {
        return interactable is InteractableLayable;
    }

    public override void StartInteract(IInteractable interactable)
    {
        lastPos = Interactor.transform.position;

        Interactor.GetComponent<CharacterStateController>().TryEnterState(CharacterState.LAYING);

        var sittable = interactable as InteractableLayable;
        Interactor.transform.SetPositionAndRotation(sittable.SitTfm.position, sittable.SitTfm.rotation);

        Locator.Resolve<IInputService>().OnMoved += OnMoved;
    }

    public override void StopInteract(IInteractable interactable)
    {
        Locator.Resolve<IInputService>().OnMoved -= OnMoved;

        Interactor.transform.position = lastPos;
        Interactor.transform.rotation = Quaternion.identity;

        Interactor.GetComponent<CharacterStateController>().TryEnterState(CharacterState.DEFAULT);
    }

    private void OnMoved(Vector2 dir)
    {
        StopInteract(null);
    }
}
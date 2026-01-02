using System;

[Serializable]
public class InteractableActionBuyTicket : InteractionAction
{
    public override bool CanInteract(IInteractable interactable)
    {
        return interactable is InteractableBuyTicket;
    }

    protected override void StartInteractInternal(IInteractable interactable)
    {
        base.StartInteractInternal(interactable);
        StopInteract(interactable);
    }
}
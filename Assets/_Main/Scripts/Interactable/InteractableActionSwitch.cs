using System;

[Serializable]
public class InteractableActionSwitch : InteractionAction
{
    public override bool CanInteract(IInteractable interactable)
    {
        return interactable.Type == InteractionType.OPEN || interactable.Type == InteractionType.CLOSE && base.CanInteract(interactable);
    }
}
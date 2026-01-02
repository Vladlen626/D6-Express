using System;

[Serializable]
public class InteractableActionEnterTrain : InteractionAction
{
    public override bool CanInteract(IInteractable interactable)
    {
        return interactable is InteractableTrainEntrance;
    }

    protected override void StartInteractInternal(IInteractable interactable)
    {
        base.StartInteractInternal(interactable);
        StopInteract(interactable);
    }
}
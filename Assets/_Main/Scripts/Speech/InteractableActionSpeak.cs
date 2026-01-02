using System;

[Serializable]
public class InteractableActionSpeak : InteractionAction
{
    public int Id { get; private set; } = -1;

    public override bool CanInteract(IInteractable interactable)
    {
        return interactable is InteractableSpeakable;
    }

    protected override void StartInteractInternal(IInteractable interactable)
    {
        base.StartInteractInternal(interactable);

        var speakable = interactable as InteractableSpeakable;
        Id = speakable.Id;
    }

    protected override void StopInteractInternal(IInteractable interactable)
    {
        Id = -1;

        base.StopInteractInternal(interactable);
    }
}
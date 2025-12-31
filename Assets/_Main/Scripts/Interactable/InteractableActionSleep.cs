using System;

[Serializable]
public class InteractableActionSleep : InteractionAction
{
    private CharacterStateController stateController;

    public override void Init(Interactor interactor)
    {
        base.Init(interactor);

        stateController = interactor.GetComponent<CharacterStateController>();
    }

    public override bool CanInteract(IInteractable interactable)
    {
        return interactable is InteractableSleepable && stateController.State == CharacterState.LAYING;
    }

    protected override void StartInteractInternal(IInteractable interactable)
    {
        base.StartInteractInternal(interactable);
    }

    protected override void StopInteractInternal(IInteractable interactable)
    {
        base.StopInteractInternal(interactable);
    }
}
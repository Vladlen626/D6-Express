public class InteractableRestock : Interactable
{
    public override InteractionType Type => InteractionType.RESTOCK;

    public override bool CanInteract(Interactor interactor)
    {
        return true;
    }

    public override void StartInteract(Interactor interactor)
    {
    }

    public override void StopInteract(Interactor interactor)
    {
    }
}

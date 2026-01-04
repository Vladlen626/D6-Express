public class InteractableTradeable : Interactable
{
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
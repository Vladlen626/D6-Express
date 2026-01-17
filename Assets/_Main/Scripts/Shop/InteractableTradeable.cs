public class InteractableTradeable : Interactable
{
    public override InteractionType Type => InteractionType.TRADE;

    public override void StartInteract(Interactor interactor)
    {
    }

    public override void StopInteract(Interactor interactor)
    {
    }
}
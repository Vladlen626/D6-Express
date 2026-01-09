public class InteractableSleepable : Interactable
{
    public override InteractionType Type => InteractionType.SLEEP;

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
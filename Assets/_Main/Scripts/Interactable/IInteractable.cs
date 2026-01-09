public interface IInteractable
{
    InteractionType Type { get; }
    
    bool CanInteract(Interactor interactor);
    void StartInteract(Interactor interactor);
    void StopInteract(Interactor interactor);
}
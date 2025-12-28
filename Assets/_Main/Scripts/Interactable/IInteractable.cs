public interface IInteractable
{
    bool CanInteract(Interactor interactor);
    void StartInteract(Interactor interactor);
    void StopInteract(Interactor interactor);
}
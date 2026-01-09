using UnityEngine;

public abstract class Interactable : MonoBehaviour, IInteractable
{
    private void Awake()
    {
        // TODO: не юзать стринг
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    public abstract InteractionType Type { get; }
    public abstract bool CanInteract(Interactor interactor);
    public abstract void StartInteract(Interactor interactor);
    public abstract void StopInteract(Interactor interactor);
}
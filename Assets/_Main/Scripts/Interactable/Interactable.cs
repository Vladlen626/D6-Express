using UnityEngine;

public abstract class Interactable : MonoBehaviour, IInteractable
{
    [SerializeField]
    private bool blockInteract;

    private void Awake()
    {
        // TODO: не юзать стринг
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    public abstract InteractionType Type { get; }

    public virtual bool CanInteract(Interactor interactor)
    {
        return !blockInteract;
    }

    public abstract void StartInteract(Interactor interactor);
    public abstract void StopInteract(Interactor interactor);
}
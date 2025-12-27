using UnityEngine;

public abstract class Interactable : MonoBehaviour, IInteractable
{
    private void Start()
    {
        // TODO: не юзать стринг
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    public abstract bool CanInteract();

    public abstract void Interact();
}

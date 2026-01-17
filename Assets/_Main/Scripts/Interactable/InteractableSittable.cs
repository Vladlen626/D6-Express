using UnityEngine;

public class InteractableSittable : Interactable
{
    [SerializeField]
    private Transform sitTfm;

    private bool occupied;

    public override InteractionType Type => InteractionType.SIT;

    public Transform SitTfm => sitTfm;

    public override bool CanInteract(Interactor interactor)
    {
        return base.CanInteract(interactor) && !occupied;
    }

    public override void StartInteract(Interactor interactor)
    {
        occupied = true;
    }

    public override void StopInteract(Interactor interactor)
    {
        occupied = false;
    }
}
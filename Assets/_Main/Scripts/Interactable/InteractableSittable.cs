using UnityEngine;

public class InteractableSittable : Interactable
{
    [SerializeField]
    private Transform sitTfm;

    public Transform SitTfm => sitTfm;

    public override bool CanInteract(Interactor interactor)
    {
        return true;
    }

    public override void StartInteract(Interactor interactor)
    {
        Debug.Log("chair occupied");
    }

    public override void StopInteract(Interactor interactor)
    {
        Debug.Log("chair is not occupied anymore");
    }
}

using UnityEngine;

public class InteractableSpeakable : Interactable
{
    // todo: надо думать что это будет динамически меняться
    [SerializeField]
    private int id = -1;

    public int Id => id;

    public override bool CanInteract(Interactor interactor)
    {
        return id != -1;
    }

    public override void StartInteract(Interactor interactor)
    {
    }

    public override void StopInteract(Interactor interactor)
    {
    }
}

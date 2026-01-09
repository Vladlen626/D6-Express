using UnityEngine;

public class InteractableSpeakable : Interactable
{
    [SerializeField] private int id = -1;

    public override InteractionType Type => InteractionType.SPEAK;
    public int Id => id;

    public override bool CanInteract(Interactor interactor) => id != -1;

    public override async void StartInteract(Interactor interactor)
    {
        GetComponent<NpcInitializer>().playerStateModel.TryAddState(CharacterState.SPEAKING);
        var rotationController = GetComponent<NpcRotationController>();
        rotationController.Target = interactor.GetComponent<Targetable>().CameraTarget.position;
    }

    public override async void StopInteract(Interactor interactor)
    {
        GetComponent<NpcInitializer>().playerStateModel.TryRemoveState(CharacterState.SPEAKING);

        var rotationController = GetComponent<NpcRotationController>();
        rotationController.Target = null;
    }
}

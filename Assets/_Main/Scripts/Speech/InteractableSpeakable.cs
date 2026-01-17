using UnityEngine;

public class InteractableSpeakable : Interactable
{
    [SerializeField] private int id = -1;

    private int currentId;

    public override InteractionType Type => InteractionType.SPEAK;
    public int Id => currentId;

    private void Awake()
    {
        currentId = id;
    }

    public void SetId(int id)
    {
        currentId = id;
    }

    public void ResetId()
    {
        currentId = id;
    }

    public override bool CanInteract(Interactor interactor) => base.CanInteract(interactor) && currentId != -1;

    public override async void StartInteract(Interactor interactor)
    {
        // todo: NpcInitializer это атавизм
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

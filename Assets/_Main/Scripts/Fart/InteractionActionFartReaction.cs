using System;

[Serializable]
public class InteractionActionFartReaction : InteractionAction
{
    private bool isInReaction;

    public override bool CanInteract(IInteractable interactable)
    {
        return base.CanInteract(interactable) && !isInReaction;
    }

    protected override async void StartInteractInternal()
    {
        base.StartInteractInternal();

        isInReaction = true;

        StateModel.TryAddState(CharacterState.SPEAKING);

        // todo: оч наивно что они на одном уровне
        // + когда то Interactor не будет юнити объектом
        var speechBubbleView = Interactor.GetComponent<SpeechBubbleView>();

        var rotationController = Interactor.GetComponent<NpcRotationController>();
        rotationController.Target = (Interactable as InteractableFart).GetComponent<Targetable>().CameraTarget.position;

        await speechBubbleView.ShowLine("fart_reaction");

        StopInteract();
    }

    protected override void StopInteractInternal()
    {
        StateModel.TryRemoveState(CharacterState.SPEAKING);

        var rotationController = Interactor.GetComponent<NpcRotationController>();
        rotationController.Target = null;

        isInReaction = false;

        base.StopInteractInternal();
    }
}
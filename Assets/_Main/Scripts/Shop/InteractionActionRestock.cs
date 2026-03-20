using System;

[Serializable]
public class InteractionActionRestock : InteractionAction
{
    private InteractableRestock cachedInteractable;
    private RestockLeverView cachedLeverView;

    public override bool CanInteract(IInteractable interactable)
    {
        return interactable.Type == InteractionType.RESTOCK && StateModel.HasState(CharacterState.DEFAULT);
    }

    protected override void StartInteractInternal(bool immediate = false)
    {
        base.StartInteractInternal(immediate);

        ResolveLeverView();
        PlayInteractableSound(SoundNames.Restock);
        cachedLeverView?.RequestRestock();
    }

    private void ResolveLeverView()
    {
        var interactableRestock = Interactable as InteractableRestock;
        if (cachedInteractable == interactableRestock && cachedLeverView)
        {
            return;
        }

        cachedInteractable = interactableRestock;
        if (!cachedInteractable)
        {
            cachedLeverView = null;
            return;
        }

        cachedLeverView = cachedInteractable.GetComponent<RestockLeverView>();
    }
}

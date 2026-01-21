using System;

[Serializable]
public class InteractionActionTrade : InteractionAction
{
    public override bool CanInteract(IInteractable interactable)
    {
        return interactable.Type == InteractionType.TRADE && StateModel.HasState(CharacterState.DEFAULT);
    }

    protected override void StartInteractInternal(bool immediate = false)
    {
        base.StartInteractInternal(immediate);

        var interactableTradeable = Interactable as InteractableTradeable;
        interactableTradeable.GetComponent<TradeItemView>().Buy();
    }
}
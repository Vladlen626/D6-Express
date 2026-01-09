using System;

[Serializable]
public class InteractionActionTrade : InteractionAction
{
    public override bool CanInteract(IInteractable interactable)
    {
        return interactable.Type == InteractionType.TRADE;
    }

    protected override void StartInteractInternal()
    {
        base.StartInteractInternal();

        var interactableTradeable = Interactable as InteractableTradeable;
        interactableTradeable.GetComponent<TradeItem>().Buy(Interactor.gameObject);
    }
}
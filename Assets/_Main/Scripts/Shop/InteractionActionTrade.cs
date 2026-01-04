using System;

[Serializable]
public class InteractionActionTrade : InteractionAction
{
    public override bool CanInteract(IInteractable interactable)
    {
        return interactable is InteractableTradeable;
    }

    protected override void StartInteractInternal()
    {
        base.StartInteractInternal();

        var interactableTradeable = Interactable as InteractableTradeable;
        interactableTradeable.GetComponent<TradeItem>().Buy(Interactor.gameObject);
    }
}
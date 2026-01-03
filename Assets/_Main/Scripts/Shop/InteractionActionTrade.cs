using System;

[Serializable]
public class InteractionActionTrade : InteractionAction
{
    public override bool CanInteract(IInteractable interactable)
    {
        return interactable is InteractableTradeable;
    }

    protected override void StartInteractInternal(IInteractable interactable)
    {
        base.StartInteractInternal(interactable);

        var interactableTradeable = interactable as InteractableTradeable;
        interactableTradeable.GetComponent<TradeItem>().Buy(Interactor.gameObject);
    }
}
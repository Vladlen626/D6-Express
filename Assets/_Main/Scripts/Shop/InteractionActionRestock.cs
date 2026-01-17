using System;

[Serializable]
public class InteractionActionRestock : InteractionAction
{
    public override bool CanInteract(IInteractable interactable)
    {
        return interactable.Type == InteractionType.RESTOCK && StateModel.HasState(CharacterState.DEFAULT);
    }

    protected override void StartInteractInternal()
    {
        base.StartInteractInternal();

        var interactableRestock = Interactable as InteractableRestock;
        interactableRestock.GetComponent<RestockLeverView>().RequestRestock();
    }
}
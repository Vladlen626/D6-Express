using System;

[Serializable]
public class InteractableActionSleep : InteractionAction
{
	public override bool CanInteract(IInteractable interactable)
	{
		return interactable.Type == InteractionType.SLEEP && !StateModel.HasState(CharacterState.SLEEPING) && base.CanInteract(interactable);
	}

	protected override async void StartInteractInternal()
	{
		StateModel.TryAddState(CharacterState.SLEEPING);

		StopInteract();
	}

	protected override async void StopInteractInternal()
	{
		StateModel.TryRemoveState(CharacterState.SLEEPING);
	}
}
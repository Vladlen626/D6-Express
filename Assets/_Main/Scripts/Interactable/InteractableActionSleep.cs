using System;

[Serializable]
public class InteractableActionSleep : InteractionAction
{
	public override bool CanInteract(IInteractable interactable)
	{
		return interactable.Type == InteractionType.SLEEP &&  base.CanInteract(interactable);
	}
	
	protected override async void StartInteractInternal(bool immediate = false)
	{
		PlayInteractableSound(SoundNames.DayChange);
		StateModel.TryAddState(CharacterState.SLEEPING);
	}

	protected override async void StopInteractInternal(bool immediate = false)
	{
		StateModel.TryRemoveState(CharacterState.SLEEPING);
	}
}

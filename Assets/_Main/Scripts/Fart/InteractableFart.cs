using System;

public class InteractableFart : Interactable
{
	public override InteractionType Type => InteractionType.FART;

	public override void StartInteract(Interactor interactor)
	{
	}

	public override void StopInteract(Interactor interactor)
	{
	}
}

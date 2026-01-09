using UnityEngine;

public class InteractableDiceGame : Interactable
{
	[SerializeField]
	private Transform sitTfm;

    public override InteractionType Type => InteractionType.PLAY_DICE;

	public Transform SitTfm => sitTfm;

	public override bool CanInteract(Interactor interactor)
	{
		return true;
	}

	public override void StartInteract(Interactor interactor)
	{
	}

	public override void StopInteract(Interactor interactor)
	{
	}
}
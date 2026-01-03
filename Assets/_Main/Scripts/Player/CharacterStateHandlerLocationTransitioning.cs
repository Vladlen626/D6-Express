using System;

[Serializable]
public class CharacterStateHandlerLocationTransitioning : CharacterStateHandler
{
	public override CharacterState State => CharacterState.LOCATION_TRANSITIONING;

	protected override void EnterInternal()
	{
		Controller.GetComponent<Interactor>().StopAllActions();
		Exit();
	}
}
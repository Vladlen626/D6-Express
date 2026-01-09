using System;

[Serializable]
public class CharacterStateHandlerLocationTransitioning : CharacterStateHandler
{
	public override CharacterState State => CharacterState.LOCATION_TRANSITIONING;

	protected override void EnterInternal()
	{
		base.EnterInternal();
		CharacterView.Interactor.StopAllActions();
		Exit();
	}
}
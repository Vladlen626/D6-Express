using System;

[Serializable]
public class CharacterStateHandlerTransition : CharacterStateHandler
{
	public override CharacterState State => CharacterState.TRANSITION;
}
using System;
using UnityEngine;

[Serializable]
public class CharacterStateHandlerSitting : CharacterStateHandler
{
	public override CharacterState State => CharacterState.SITTING;
}
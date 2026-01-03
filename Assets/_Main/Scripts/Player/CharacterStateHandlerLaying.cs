using System;
using UnityEngine;

[Serializable]
public class CharacterStateHandlerLaying : CharacterStateHandler
{
	public override CharacterState State => CharacterState.LAYING;
}
using System;
using UnityEngine;

[Serializable]
public class CharacterStateHandlerLaying : CharacterStateHandler
{
	public override CharacterState State => CharacterState.LAYING;

    protected override void EnterInternal()
    {
        base.EnterInternal();
    }

    protected override void ExitInternal()
    {
        base.ExitInternal();
    }
}
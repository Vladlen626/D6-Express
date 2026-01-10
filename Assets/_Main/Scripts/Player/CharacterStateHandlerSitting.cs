using System;
using UnityEngine;

[Serializable]
public class CharacterStateHandlerSitting : CharacterStateHandler
{
	public override CharacterState State => CharacterState.SITTING;

	protected override void EnterInternal()
	{
		// base.EnterInternal();

		CharacterView.GetComponent<Animator>().SetInteger("State", (int)CharacterState.SITTING);
	}

	protected override void ExitInternal()
	{
		CharacterView.GetComponent<Animator>().SetInteger("State", (int)CharacterState.DEFAULT);

		// base.ExitInternal();
	}
}
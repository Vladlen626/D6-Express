using System;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using UnityEngine;

[Serializable]
public class CharacterStateHandlerSleeping : CharacterStateHandler
{
	private IInputService inputService;

	public override CharacterState State => CharacterState.SLEEPING;

	public override void OnInit()
	{
		inputService = Locator.Resolve<IInputService>();
	}

	protected override void EnterInternal()
	{
		// todo: это хак чтобы не включать капсулу
		// base.EnterInternal();
		inputService.DisableAllInputs();
	}

	protected override void ExitInternal()
	{
		// todo: это хак
		// base.ExitInternal();
		inputService.EnableAllInputs();
	}
}
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

	public override void Enter()
	{
		base.Enter();
		inputService.DisableAllInputs();
	}

	public override void Exit()
	{
		base.Exit();
		inputService.EnableAllInputs();
	}
}
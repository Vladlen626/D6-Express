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
		inputService.DisablePlayerInputs();
		inputService.DisableCameraInputs();
	}

	protected override void ExitInternal()
	{
		inputService.EnablePlayerInputs();
		inputService.EnableCameraInputs();
	}
}
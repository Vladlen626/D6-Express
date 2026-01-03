using System;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Services.UI;
using UnityEngine;

[Serializable]
public class CharacterStateHandlerSpeaking : CharacterStateHandler
{
	public override CharacterState State => CharacterState.SPEAKING;
	
	private ICursorService cursorService;
	private IInputService inputService;

	public override void OnInit()
	{
		cursorService = Locator.Resolve<ICursorService>();
		inputService = Locator.Resolve<IInputService>();
	}

	public override void Enter()
	{
		base.Enter();
		inputService.DisableCameraInputs();
		cursorService.UnlockCursor();
	}

	public override void Exit()
	{
		base.Exit();
		inputService.EnableCameraInputs();
		cursorService.LockCursor();
	}
}
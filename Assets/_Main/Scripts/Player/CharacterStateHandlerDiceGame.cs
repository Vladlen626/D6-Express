using System;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Services.UI;
using UnityEngine;

[Serializable]
public class CharacterStateHandlerDiceGame : CharacterStateHandler
{
	public override CharacterState State => CharacterState.DICE_GAME;
	private ICursorService cursorService;
	private IInputService inputService;

	public override void OnInit()
	{
		cursorService = Locator.Resolve<ICursorService>();
		inputService = Locator.Resolve<IInputService>();
	}

	public override void Enter()
	{
		Controller.GetComponent<CharacterController>().enabled = false;
		Controller.GetComponent<Collider>().enabled = false;
		inputService.DisableCameraInputs();
		cursorService.UnlockCursor();
	}

	public override void Exit()
	{
		Controller.GetComponent<CharacterController>().enabled = true;
		Controller.GetComponent<Collider>().enabled = true;
		inputService.EnableCameraInputs();
		cursorService.LockCursor();
	}
}
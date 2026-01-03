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

	protected override void EnterInternal()
	{
		Controller.GetComponent<CharacterController>().enabled = false;
		Controller.GetComponent<Collider>().enabled = false;
		inputService.DisableCameraInputs();
		cursorService.UnlockCursor();
	}

	protected override void ExitInternal()
	{
		Controller.GetComponent<CharacterController>().enabled = true;
		Controller.GetComponent<Collider>().enabled = true;
		inputService.EnableCameraInputs();
		cursorService.LockCursor();
	}
}
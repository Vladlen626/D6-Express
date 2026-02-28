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
		base.EnterInternal();
		
		inputService.DisableCameraInputs();
		inputService.DisableUIInputs();
	}

	protected override void ExitInternal()
	{
		inputService.EnableCameraInputs();
		inputService.EnableUIInputs();

		base.ExitInternal();
	}
}
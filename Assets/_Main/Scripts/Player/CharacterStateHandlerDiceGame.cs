using System;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;

[Serializable]
public class CharacterStateHandlerDiceGame : CharacterStateHandler
{
	public override CharacterState State => CharacterState.DICE_GAME;
	private ICursorService cursorService;
	private IInputService inputService;

	public override void Start()
	{
		cursorService = Locator.Resolve<ICursorService>();
		inputService = Locator.Resolve<IInputService>();
	}

	public override void Enter()
	{
		inputService.DisableCameraInputs();
		cursorService.UnlockCursor();
	}

	public override void Exit()
	{
		inputService.EnableCameraInputs();
		cursorService.LockCursor();
	}
}
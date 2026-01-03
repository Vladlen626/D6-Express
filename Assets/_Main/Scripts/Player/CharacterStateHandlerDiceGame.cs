using System;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Services.UI;

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

	protected override void EnterInternal()
	{
		base.Enter();
		inputService.DisableCameraInputs();
		cursorService.UnlockCursor();
	}

    protected override void ExitInternal()
	{
		base.Exit();
		inputService.EnableCameraInputs();
		cursorService.LockCursor();
	}
}
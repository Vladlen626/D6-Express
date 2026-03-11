using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.UI;

public class CursorHotkeyController : IBaseController, IActivatable
{
	private readonly IInputService inputService;
	private readonly ICursorService cursorService;

	public CursorHotkeyController(IInputService inputService, ICursorService cursorService)
	{
		this.inputService = inputService;
		this.cursorService = cursorService;
	}

	public void Activate()
	{
		inputService.OnJumpPressed += OnJumpPressed;
	}

	public void Deactivate()
	{
		inputService.OnJumpPressed -= OnJumpPressed;
	}

	private void OnJumpPressed()
	{
		cursorService.ForceToggleCursor();
	}
}

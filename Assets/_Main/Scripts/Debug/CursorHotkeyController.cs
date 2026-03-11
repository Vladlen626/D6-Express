using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.UI;
using UnityEngine;

public class CursorHotkeyController : IBaseController, IActivatable, IUpdatable
{
	private readonly ICursorService cursorService;
	private bool isActive;

	public CursorHotkeyController(ICursorService cursorService)
	{
		this.cursorService = cursorService;
	}

	public void Activate()
	{
		isActive = true;
	}

	public void Deactivate()
	{
		isActive = false;
	}

	public void OnUpdate(float deltaTime)
	{
		if (!isActive)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.K))
		{
			cursorService.ForceToggleCursor();
		}
	}
}

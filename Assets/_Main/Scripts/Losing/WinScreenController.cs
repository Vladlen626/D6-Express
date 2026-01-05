using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Services.UI;

public class WinScreenController : BaseContextController<UIWinView>
{
	private readonly IInputService inputService;
	private readonly ICursorService cursorService;
	private readonly LevelModel levelModel;

	public WinScreenController(IUIService uiService, IInputService inputService, ICursorService cursorService,
		LevelModel levelModel) : base(uiService)
	{
		this.inputService = inputService;
		this.cursorService = cursorService;
		this.levelModel = levelModel;
	}

	protected override void OnActivate()
	{
		base.OnActivate();

		HideContext();

		levelModel.LevelFinished += LevelFinishedHandler;
		_context.ExitButtonClicked += OnExitButtonClickedHandler;
	}

	protected override void OnDeactivate()
	{
		_context.ExitButtonClicked -= OnExitButtonClickedHandler;
		levelModel.LevelFinished -= LevelFinishedHandler;

		HideContext();

		base.OnDeactivate();
	}

	private void LevelFinishedHandler(bool result)
	{
		// if (result)
		// {
		// 	if (DebugVariables.ShowWinView)
		// 	{
		// 		ShowContext();
		// 	}
		// 	else
		// 	{
		// 		levelModel.SetLevelState(LevelState.STATION);
		// 	}
		// }
	}

	private void ShowContext()
	{
		_context.Show();
		inputService.DisablePlayerInputs();
		cursorService.UnlockCursor();
	}

	private void HideContext()
	{
		_context.Hide();
		inputService.EnablePlayerInputs();
		cursorService.LockCursor();
	}

	private void OnExitButtonClickedHandler()
	{
		levelModel.SetLevelState(LevelState.STATION);
		_context.Hide();
	}
}
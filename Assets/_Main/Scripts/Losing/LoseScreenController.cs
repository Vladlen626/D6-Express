using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Services.UI;

public class LoseScreenController : BaseContextController<UILoseView>
{
	private readonly IInputService inputService;
	private readonly ICursorService cursorService;
	private readonly RunModel runModel;

	public LoseScreenController(IUIService uiService, IInputService inputService, ICursorService cursorService,
		RunModel runModel) : base(uiService)
	{
		this.inputService = inputService;
		this.cursorService = cursorService;
		this.runModel = runModel;
	}

	protected override void OnActivate()
	{
		base.OnActivate();

		HideContext();

		runModel.LevelModel.LevelFinished += LevelFinishedHandler;
		_context.ExitButtonClicked += OnExitButtonClickedHandler;
		_context.ContinueButtonClicked += OnContinueButtonClickedHandler;
	}

	protected override void OnDeactivate()
	{
		_context.ContinueButtonClicked -= OnContinueButtonClickedHandler;
		_context.ExitButtonClicked -= OnExitButtonClickedHandler;
		runModel.LevelModel.LevelFinished -= LevelFinishedHandler;

		HideContext();

		base.OnDeactivate();
	}

	private void LevelFinishedHandler(bool result)
	{
		if (!result)
		{
			if (DebugVariables.ShowLoseView)
			{
				ShowContext();
			}
			else
			{
				runModel.SetLevelState(LevelState.STATION);
			}
		}
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
		runModel.SetLevelState(LevelState.STATION);
		_context.Hide();
	}

	private void OnContinueButtonClickedHandler()
	{
		runModel.SetLevelState(LevelState.STATION);
		_context.Hide();
	}
}
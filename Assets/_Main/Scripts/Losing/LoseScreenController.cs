using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Services.UI;

public class LoseScreenController : BaseContextController<UILoseView>
{
	private readonly IInputService inputService;
	private readonly ICursorService cursorService;
	private readonly LevelModel levelModel;

	public LoseScreenController(IUIService uiService, IInputService inputService, ICursorService cursorService,
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

		levelModel.LevelFinished += OnLevelFinishedHandler;

		_context.ExitButtonClicked += OnExitButtonClickedHandler;
		_context.ContinueButtonClicked += OnContinueButtonClickedHandler;
	}

	override protected void OnDeactivate()
	{
		_context.ContinueButtonClicked -= OnContinueButtonClickedHandler;
		_context.ExitButtonClicked -= OnExitButtonClickedHandler;

		levelModel.LevelFinished -= OnLevelFinishedHandler;

		HideContext();

		base.OnDeactivate();
	}

	private void OnLevelFinishedHandler(bool result)
	{
		if (!result)
		{
			if (DebugVariables.ShowLoseView)
			{
				ShowContext();
			}
			else
			{
				levelModel.SetLevelState(LevelState.STATION);
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
		levelModel.SetLevelState(LevelState.STATION);
		_context.Hide();
	}

	private void OnContinueButtonClickedHandler()
	{
		levelModel.SetLevelState(LevelState.STATION);
		_context.Hide();
	}
}
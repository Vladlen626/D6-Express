using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;

public class LoseViewController : BaseContextController<UILoseView>
{
	private readonly IInputService inputService;
	private readonly ICursorService cursorService;
	private readonly RunModel runModel;
	private readonly ConfigService configService;

	private TextsConfig textsConfig;

	public LoseViewController(IUIService uiService, IInputService inputService, ICursorService cursorService,
		RunModel runModel, ConfigService configService) : base(uiService)
	{
		this.inputService = inputService;
		this.cursorService = cursorService;
		this.runModel = runModel;
		this.configService = configService;
	}

	protected override async UniTask OnPreloadAsync()
	{
		textsConfig = await configService.GetFirstOrDefaultAsync<TextsConfig>(ResourcePaths.Json.texts_eng);
	}

	protected override void OnActivate()
	{
		base.OnActivate();

		_context.SetLoseText(textsConfig.texts["lose_header"]);
		_context.SetExitButtonText(textsConfig.texts["exit_button"]);
		_context.SetContinueButtonText(textsConfig.texts["continue_button"]);

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
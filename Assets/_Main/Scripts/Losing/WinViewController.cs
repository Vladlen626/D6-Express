using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;

public class WinViewController : BaseContextController<UIWinView>
{
	private readonly IInputService inputService;
	private readonly ICursorService cursorService;
	private readonly RunModel runModel;
	private readonly ConfigService configService;

	private TextsConfig textsConfig;

	public WinViewController(IUIService uiService, IInputService inputService, ICursorService cursorService,
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

		_context.SetWinText(textsConfig.texts["win_header"]);
		_context.SetExitButtonText(textsConfig.texts["exit_button"]);

		HideContext();

		runModel.Finished += RunFinished;
		_context.ExitButtonClicked += OnExitButtonClickedHandler;
	}

	protected override void OnDeactivate()
	{
		_context.ExitButtonClicked -= OnExitButtonClickedHandler;
		runModel.Finished -= RunFinished;

		HideContext();

		base.OnDeactivate();
	}

	private void RunFinished(bool result)
	{
		if (result)
		{
#if UNITY_EDITOR
			if (DebugVariables.ShowWinView)
			{
				ShowContext();
			}
			else
			{
				runModel.SetLevelState(LevelState.STATION);
			}
#else
			runModel.SetLevelState(LevelState.STATION);
#endif
			
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
}
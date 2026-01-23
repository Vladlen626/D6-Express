using System.Linq;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;

public class WinViewController : BaseContextController<UIWinView>
{
	private readonly IInputService inputService;
	private readonly ICursorService cursorService;
	private readonly ConfigService configService;
    private readonly TransitionService transitionService;
    private TextsConfig textsConfig;

	public WinViewController(IUIService uiService, IInputService inputService, ICursorService cursorService, ConfigService configService, TransitionService transitionService) : base(uiService)
	{
		this.inputService = inputService;
		this.cursorService = cursorService;
		this.configService = configService;
        this.transitionService = transitionService;
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

		transitionService.TransitionRequested += OnTransitionRequested;
		_context.ExitButtonClicked += OnExitButtonClickedHandler;
	}

	protected override void OnDeactivate()
	{
		_context.ExitButtonClicked -= OnExitButtonClickedHandler;
		transitionService.TransitionRequested -= OnTransitionRequested;

		HideContext();

		base.OnDeactivate();
	}

	private void OnTransitionRequested()
	{
		if (transitionService.CurrentTransition.data.tasks.Contains(Transition.TaskType.WIN))
		{
			transitionService.CurrentTransition.AddTask(async () => ShowContext());
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
		_context.Hide();
	}
}
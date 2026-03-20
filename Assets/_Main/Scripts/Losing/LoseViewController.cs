using System;
using System.Collections.Generic;
using System.Linq;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;

public class LoseViewController : BaseContextController<UIEndView>, IGameStateChanger
{
	private readonly D6Game game;
	private readonly IInputService inputService;
	private readonly ICursorService cursorService;
	private readonly ConfigService configService;
	private TextsConfig textsConfig;

	public LoseViewController(IUIService uiService, D6Game game, IInputService inputService, ICursorService cursorService, ConfigService configService) : base(uiService)
	{
		this.game = game;
		this.inputService = inputService;
		this.cursorService = cursorService;
		this.configService = configService;
	}

	public IEnumerable<(GameStateTransitionTask task, GameStateChangeFunc func)> GetStateChangeFuncs()
	{
		yield return (GameStateTransitionTask.SHOW_LOSE, async (x) => ShowContext());
		yield return (GameStateTransitionTask.HIDE_LOSE, async (x) => HideContext());
	}

	protected override async UniTask OnPreloadAsync()
	{
		textsConfig = await configService.GetFirstOrDefaultAsync<TextsConfig>(ResourcePaths.Json.texts_eng);
	}

	protected override void OnActivate()
	{
		base.OnActivate();

		_context.Hide();

		_context.ExitButtonClicked += OnExitButtonClickedHandler;
	}

	protected override void OnDeactivate()
	{
		_context.ExitButtonClicked -= OnExitButtonClickedHandler;
		_context.Hide();

		base.OnDeactivate();
	}

	private void ShowContext()
	{
		_context.SetTitle(textsConfig.texts["end_header"]);
		_context.SetMessage(textsConfig.texts["lose_header"]);
		_context.SetExitButtonText(textsConfig.texts["exit_button"]);
		_context.SetWinImage(false);
		_context.SetLoseImage(true);
		_context.SetPostcardColor(_context.colorLose);

		_context.Show();
		_context.PlayLoseCashAnimation();
	}

	private void HideContext()
	{
		_context.Hide();
	}

	private void OnExitButtonClickedHandler()
	{
		game.RequestSetLocation(Location.MAIN_MENU);
	}
}
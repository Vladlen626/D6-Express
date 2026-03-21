using PlatformCore.Core;
using PlatformCore.Services.UI;

namespace _Main.Scripts.UI
{
	public class MainMenuController : BaseContextController<UIMainMenu>
	{
		private readonly D6Game game;
		private readonly Run run;

		public MainMenuController(IUIService uiService, D6Game game, Run run)
			: base(uiService)
		{
			this.game = game;
			this.run = run;
		}

		protected override void OnActivate()
		{
			_context.Hide();

			_context.OnStartClicked += OnStartClickedHandler;
			_context.OnSettingsClicked += OnSettingsClickedHandler;

			game.LocationChanged += OnLocationChanged;
		}

		protected override void OnDeactivate()
		{
			game.LocationChanged -= OnLocationChanged;

			_context.OnStartClicked -= OnStartClickedHandler;
			_context.OnSettingsClicked -= OnSettingsClickedHandler;
		}

		private void OnLocationChanged()
		{
			if (game.Location == Location.MAIN_MENU)
			{
				_context.Show();
			}
			else
			{
				_context.Hide();
			}
		}

		private void OnStartClickedHandler()
		{
			if (!run.Started)
			{
				_context.Hide();
				run.RequestStart();
			}
		}

		private void OnSettingsClickedHandler()
		{
		}
	}
}
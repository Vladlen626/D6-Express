using PlatformCore.Core;
using PlatformCore.Services.UI;

namespace _Main.Scripts.UI
{
	public class DifficultyLevelController : BaseContextController<UIDifficultyLevel>
	{
		private readonly D6Game game;
		private readonly Run run;

		public DifficultyLevelController(IUIService uiService, D6Game game, Run run) : base(uiService)
		{
			this.game = game;
			this.run = run;
		}

		protected override void OnActivate()
		{
			_context.Hide();

			_context.OnEasyClicked += OnEasyClickedHandler;
			_context.OnHardClicked += OnHardClickedHandler;

			run.StartRequested += OnStartRequested;
			game.LocationChanged += OnLocationChanged;
		}

		protected override void OnDeactivate()
		{
			game.LocationChanged -= OnLocationChanged;
			run.StartRequested -= OnStartRequested;

			_context.OnHardClicked -= OnHardClickedHandler;
			_context.OnEasyClicked -= OnEasyClickedHandler;
		}

		private void OnStartRequested()
		{
			if (run.Started || game.Location != Location.MAIN_MENU)
			{
				return;
			}

			_context.Show();
		}

		private void OnLocationChanged()
		{
			if (game.Location != Location.MAIN_MENU)
			{
				_context.Hide();
			}
		}

		private void OnEasyClickedHandler()
		{
			StartRunWithRules(Run.DefaultRulesId);
		}

		private void OnHardClickedHandler()
		{
			StartRunWithRules(Run.HardRulesId);
		}

		private void StartRunWithRules(string runRulesId)
		{
			if (run.Started)
			{
				return;
			}

			run.SetRunRulesId(runRulesId);
			_context.Hide();
			run.Start();
		}
	}
}

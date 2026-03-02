using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Services.Audio;
using PlatformCore.Services.UI;

namespace _Main.Scripts.UI
{
	public class SettingsController : BaseContextController<UISettings>
	{
		private readonly IUIService uiService;
		private readonly IAudioService audioService;
		private readonly ICursorService cursorService;
		private readonly IInputService inputService;
		private readonly PauseState pauseState;

		public SettingsController(IUIService uiService, IAudioService audioService, ICursorService cursorService,
			IInputService inputService, PauseState pauseState) : base(uiService)
		{
			this.uiService = uiService;
			this.audioService = audioService;
			this.cursorService = cursorService;
			this.inputService = inputService;
			this.pauseState = pauseState;
		}

		protected override void OnActivate()
		{
			base.OnActivate();
			_context.Hide();
			inputService.OnPausePressed += OnPausePressedHandler;
			_context.OnMasterChanged += OnMasterChangedHandler;
			_context.OnMusicChanged += OnMusicChangedHandler;
			_context.OnSfxChanged += OnSfxChangedHandler;
			_context.OnCloseClicked += OnCloseClickHandler;
		}

		protected override void OnDeactivate()
		{
			inputService.OnPausePressed -= OnPausePressedHandler;
			_context.OnMasterChanged -= OnMasterChangedHandler;
			_context.OnMusicChanged -= OnMusicChangedHandler;
			_context.OnSfxChanged -= OnSfxChangedHandler;
			_context.OnCloseClicked -= OnCloseClickHandler;

			base.OnDeactivate();
		}

		private void OnPausePressedHandler()
		{
			if (_context.IsShown())
			{
				HideContext();
			}
			else
			{
				_context.SetValues(audioService.MasterVolume, audioService.MusicVolume, audioService.SfxVolume);
				ShowContext();
			}
		}

		private void ShowContext()
		{
			_context.Show();
			inputService.DisablePlayerInputs();
			cursorService.UnlockCursor();
			pauseState.SetPaused(true);
		}

		private void HideContext()
		{
			_context.Hide();
			inputService.EnablePlayerInputs();
			cursorService.LockCursor();
			pauseState.SetPaused(false);
		}

		private void OnMasterChangedHandler(float obj)
		{
			audioService.SetMasterVolume(obj);
		}

		private void OnMusicChangedHandler(float obj)
		{
			audioService.SetMusicVolume(obj);
		}

		private void OnSfxChangedHandler(float obj)
		{
			audioService.SetSfxVolume(obj);
		}

		private void OnCloseClickHandler()
		{
			HideContext();
		}
	}
}

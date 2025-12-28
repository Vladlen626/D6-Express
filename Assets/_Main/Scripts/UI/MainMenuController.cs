using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.UI;

namespace _Main.Scripts.UI
{
	public class MainMenuController : BaseContextController<UIMainMenu>
	{
		private UniTaskCompletionSource _startTcs;

		public MainMenuController(IUIService uiService)
			: base(uiService)
		{
		}

		protected override void OnActivate()
		{
			_context.OnStartClicked += OnStartClickedHandler;
			_context.OnSettingsClicked += OnSettingsClickedHandler;
		}

		protected override void OnDeactivate()
		{
			_context.OnStartClicked -= OnStartClickedHandler;
			_context.OnSettingsClicked -= OnSettingsClickedHandler;
		}

		public UniTask WaitForStartAsync()
		{
			_startTcs = new UniTaskCompletionSource();
			return _startTcs.Task;
		}

		private void OnStartClickedHandler()
		{
			_startTcs?.TrySetResult();
		}

		private void OnSettingsClickedHandler()
		{
		}
	}
}
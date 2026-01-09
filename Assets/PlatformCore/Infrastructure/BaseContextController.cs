using Cysharp.Threading.Tasks;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.UI;

namespace PlatformCore.Core
{
	public class BaseContextController<T> : IBaseController, IActivatable, IPreloadable where T : UIBaseElement
	{
		protected readonly IUIService _uiService;
		protected T _context;

		protected BaseContextController(IUIService uiService)
		{
			_uiService = uiService;
		}

		public async UniTask PreloadAsync()
		{
			await _uiService.PreloadAsync<T>();
			await OnPreloadAsync();
		}
		public void Activate()
		{
			_context = _uiService.GetWindow<T>();
			OnActivate();
		}
		public void Deactivate()
		{
			_uiService.Unload<T>();
			OnDeactivate();
		}
		
		protected virtual void OnActivate(){}
		protected virtual void OnDeactivate(){}

		protected virtual UniTask OnPreloadAsync()
		{
			return UniTask.CompletedTask;
		}
	}
}
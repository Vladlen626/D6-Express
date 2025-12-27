using System.Threading;
using Cysharp.Threading.Tasks;

namespace PlatformCore.Services.UI
{
	public interface IUIService
	{
		T GetWindow<T>() where T : BaseUIElement;
		bool IsShowed<T>() where T : BaseUIElement;

		void Unload<T>() where T : BaseUIElement;

		UniTask PreloadAsync<T>() where T : BaseUIElement;
	}
}
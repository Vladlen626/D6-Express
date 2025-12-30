using System.Threading;
using Cysharp.Threading.Tasks;

namespace PlatformCore.Services.UI
{
	public interface IUIService
	{
		T GetWindow<T>() where T : UIBaseElement;
		bool IsShowed<T>() where T : UIBaseElement;

		void Unload<T>() where T : UIBaseElement;

		UniTask PreloadAsync<T>() where T : UIBaseElement;
	}
}
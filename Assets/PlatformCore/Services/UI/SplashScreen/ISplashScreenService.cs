using Cysharp.Threading.Tasks;

namespace PlatformCore.Services.UI.SplashScreen
{
	public interface ISplashScreenService
	{
		UniTask FadeInAsync(float duration = 1f);
		UniTask FadeOutAsync(float duration = 1f);
		UniTask ShowSplashAsync(float duration = 2f, float fadeIn = 0.5f, float fadeOut = 0.5f);
		bool IsTransitioning { get; }
	}
}
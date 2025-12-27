using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace PlatformCore.Core
{
	public static class ServiceLocatorExtensions
	{
		public static async UniTask InitializeAllAsync(this ServiceLocator locator, CancellationToken ct)
		{
			var asyncServices = locator.All.OfType<IAsyncInitializable>().ToList();
			var syncServices = locator.All.OfType<ISyncInitializable>().ToList();

			await UniTask.WhenAll(asyncServices.Select(s => s.PreInitializeAsync(ct)));

			foreach (var svc in asyncServices)
			{
				await svc.PostInitializeAsync(ct);
			}

			foreach (var svc in syncServices)
			{
				svc.Initialize();
			}
		}
	}
}
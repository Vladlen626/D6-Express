using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlatformCore.Services
{
	public class ResourceService : BaseAsyncService, IResourceService
	{
		private readonly ILoggerService _loggerService;

		public ResourceService(ILoggerService loggerService = null)
		{
			_loggerService = loggerService;
		}

		public UniTask InitializeAsync(CancellationToken cancellationToken)
		{
			_loggerService?.Log("[ResourceService] Initialized");

			return UniTask.CompletedTask;
		}

		public async UniTask<T> LoadAsync<T>(string path) where T : Object
		{
			_loggerService?.Log($"[ResourceService] Loading: {path}");

			var request = Resources.LoadAsync<T>(path);

			await request;

			if (request.asset == null)
			{
				_loggerService?.Log($"[ResourceService] Failed to load: {path}");
				_loggerService?.LogError($"Possible reasons: 1) File not in Resources folder, 2) Wrong path, 3) Wrong type");
			}

			_loggerService?.Log($"[ResourceService] Loaded: {path}");
			return request.asset as T;
		}

		public void Unload(Object obj)
		{
			if (obj == null)
			{
				_loggerService?.LogWarning("[ResourceService] Trying to unload null object");
				return;
			}

			_loggerService?.Log($"[ResourceService] Marking for unload: {obj}");

			Resources.UnloadAsset(obj);
		}

		public async UniTask UnloadUnusedAssetsAsync()
		{
			_loggerService?.Log($"[ResourceService] Starting memory cleanup...");

			var operation = Resources.UnloadUnusedAssets();

			await operation;

			System.GC.Collect();

			_loggerService?.Log($"[ResourceService] Memory cleanup complete.");
		}

		public override void Dispose()
		{
			_loggerService?.Log("[ResourceService] Disposed");

			Resources.UnloadUnusedAssets();
		}
	}
}
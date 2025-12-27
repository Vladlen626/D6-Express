using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlatformCore.Services.Factory.PlatformCore.Services.Factory
{
	public class ObjectFactory : BaseAsyncService, IObjectFactory
	{
		private readonly IResourceService _resourceService;
		private readonly ILoggerService _loggerService;

		public ObjectFactory(IResourceService resourceService, ILoggerService loggerService)
		{
			_resourceService = resourceService;
			_loggerService = loggerService;
		}

		public async UniTask<GameObject> CreateAsync(string address, Vector3 position, Quaternion rotation,
			Transform parent = null)
		{
			_loggerService?.Log($"[ObjectFactory] Creating GameObject from: {address}");

			var prefab = await _resourceService.LoadAsync<GameObject>(address);

			if (prefab == null)
			{
				_loggerService?.LogError($"[ObjectFactory] ❌ Failed to load prefab at '{address}' (type: GameObject)");
				return null;
			}

			var instance = parent != null
				? Object.Instantiate(prefab, position, rotation, parent)
				: Object.Instantiate(prefab, position, rotation);

			instance.name = prefab.name;
			_loggerService?.Log($"[ObjectFactory] ✅ Created GameObject '{instance.name}' at {position}");
			return instance;
		}

		public async UniTask<T> CreateAsync<T>(string address, Vector3 position, Quaternion rotation, Transform parent = null)
			where T : Component
		{
			_loggerService?.Log($"[ObjectFactory] Creating component '{typeof(T).Name}' from: {address}");

			var gameObject = await CreateAsync(address, position, rotation, parent);
			if (gameObject == null)
			{
				_loggerService?.LogError($"[ObjectFactory] ❌ Prefab not found for '{typeof(T).Name}' at '{address}'");
				return null;
			}

			var component = gameObject.GetComponent<T>();
			if (component == null)
			{
				_loggerService?.LogError($"[ObjectFactory] ❌ Component '{typeof(T).Name}' missing on prefab '{address}'");
				Object.Destroy(gameObject);
				return null;
			}

			_loggerService?.Log($"[ObjectFactory] ✅ Component '{typeof(T).Name}' loaded successfully from '{address}'");
			return component;
		}

		public void Destroy(GameObject obj)
		{
			if (obj == null)
			{
				_loggerService?.LogWarning("[ObjectFactory] ⚠️ Tried to destroy null object");
				return;
			}

			_loggerService?.Log($"[ObjectFactory] Destroying: {obj.name}");
			Object.Destroy(obj);
		}
	}
}
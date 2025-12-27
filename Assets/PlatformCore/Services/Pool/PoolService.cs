using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace PlatformCore.Services.Pool
{
	public class PoolService : IPoolService, IService
	{
		private readonly ILoggerService _logger;
		private readonly IResourceService _resourceService;
		private readonly Transform _poolRoot;

		private readonly Dictionary<Type, IObjectPool> _pools = new();

		public PoolService(ILoggerService logger, IResourceService resourceService, Transform poolRoot)
		{
			_logger = logger;
			_resourceService = resourceService;
			_poolRoot = poolRoot;
		}

		public async UniTask CreatePoolAsync<T>(string prefabPath, int initialSize = 10, Transform parent = null)
			where T : Component
		{
			var type = typeof(T);

			if (_pools.ContainsKey(type))
			{
				_logger?.LogWarning($"[PoolService] Pool for {type.Name} already exists");
				return;
			}

			_logger?.Log($"[PoolService] Creating pool for {type.Name}: {initialSize} objects from {prefabPath}");

			var prefab = await _resourceService.LoadAsync<GameObject>(prefabPath);
			if (prefab == null)
			{
				_logger?.LogError($"[PoolService] Failed to load prefab: {prefabPath}");
				throw new InvalidOperationException($"Cannot create pool: prefab not found at {prefabPath}");
			}

			var prefabComponent = prefab.GetComponent<T>();
			if (prefabComponent == null)
			{
				_logger?.LogError($"[PoolService] Component {type.Name} not found on prefab {prefabPath}");
				throw new InvalidOperationException($"Component {type.Name} not found on prefab");
			}

			var poolParent = parent ?? CreatePoolParent<T>();
			var pool = new ObjectPool<T>(prefab, initialSize, poolParent, _logger);
			_pools[type] = pool;

			_logger?.Log($"[PoolService] Pool created for {type.Name}: {initialSize} objects pre-instantiated");
		}

		public T Rent<T>(Vector3 position = default, Quaternion rotation = default, Transform parent = null)
			where T : Component
		{
			var type = typeof(T);

			if (!_pools.TryGetValue(type, out var poolInterface))
			{
				_logger?.LogError($"[PoolService] Pool for {type.Name} does not exist. Call CreatePoolAsync first.");
				throw new InvalidOperationException($"Pool for {type.Name} not found. Create pool first.");
			}

			var pool = (ObjectPool<T>)poolInterface;
			var obj = pool.Rent();

			obj.transform.position = position;
			obj.transform.rotation = rotation;

			if (parent != null)
				obj.transform.SetParent(parent);

			return obj;
		}

		public void Return<T>(T component) where T : Component
		{
			if (component == null)
			{
				_logger?.LogWarning("[PoolService] Trying to return null component");
				return;
			}

			var type = typeof(T);

			if (!_pools.TryGetValue(type, out var poolInterface))
			{
				_logger?.LogWarning($"[PoolService] Pool for {type.Name} not found, destroying object instead");
				Object.Destroy(component.gameObject);
				return;
			}

			var pool = (ObjectPool<T>)poolInterface;
			pool.Return(component);
		}

		public void ReturnDelayed<T>(T component, float delay) where T : Component
		{
			if (component == null)
			{
				_logger?.LogWarning("[PoolService] Trying to return null component with delay");
				return;
			}

			ReturnDelayedAsync(component, delay).Forget();
		}

		private async UniTask ReturnDelayedAsync<T>(T component, float delay) where T : Component
		{
			try
			{
				await UniTask.Delay(TimeSpan.FromSeconds(delay));
				Return(component);
			}
			catch (Exception ex)
			{
				_logger?.LogError($"[PoolService] ReturnDelayed failed for {typeof(T).Name}: {ex.Message}");
			}
		}

		public (int active, int inactive) GetPoolStats<T>() where T : Component
		{
			var type = typeof(T);

			if (!_pools.TryGetValue(type, out var poolInterface))
			{
				return (0, 0);
			}

			var pool = (ObjectPool<T>)poolInterface;
			return pool.GetStats();
		}

		public void ClearPool<T>() where T : Component
		{
			var type = typeof(T);

			if (_pools.TryGetValue(type, out var poolInterface))
			{
				var pool = (ObjectPool<T>)poolInterface;
				pool.Clear();
				_pools.Remove(type);

				_logger?.Log($"[PoolService] Pool cleared: {type.Name}");
			}
		}

		private Transform CreatePoolParent<T>() where T : Component
		{
			var poolName = $"Pool_{typeof(T).Name}";
			var poolObject = new GameObject(poolName);
			poolObject.transform.SetParent(_poolRoot);
			return poolObject.transform;
		}

		public void Dispose()
		{
			foreach (var pool in _pools.Values)
			{
				pool.Clear();
			}

			_pools.Clear();

			_logger?.Log("[PoolService] All pools cleared and disposed");
		}
	}
}
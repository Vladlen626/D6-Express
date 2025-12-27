using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PlatformCore.Services.Pool
{
	internal interface IObjectPool
	{
		void Clear();
	}

	internal class ObjectPool<T> : IObjectPool where T : Component
	{
		private readonly GameObject _prefab;
		private readonly Transform _parent;
		private readonly ILoggerService _logger;

		private readonly Queue<T> _inactiveObjects = new();
		private readonly HashSet<T> _activeObjects = new();

		public ObjectPool(GameObject prefab, int initialSize, Transform parent, ILoggerService logger)
		{
			_prefab = prefab;
			_parent = parent;
			_logger = logger;
			
			for (int i = 0; i < initialSize; i++)
			{
				var obj = CreateNewObject();
				obj.gameObject.SetActive(false);
				_inactiveObjects.Enqueue(obj);
			}
		}

		public T Rent()
		{
			T obj;

			if (_inactiveObjects.Count > 0)
			{
				obj = _inactiveObjects.Dequeue();
			}
			else
			{
				_logger?.Log($"[ObjectPool] Pool empty, creating new {typeof(T).Name}");
				obj = CreateNewObject();
			}
			
			obj.gameObject.SetActive(true);
			_activeObjects.Add(obj);

			return obj;
		}

		public void Return(T obj)
		{
			if (obj == null || !_activeObjects.Contains(obj))
			{
				_logger?.LogWarning($"[ObjectPool] Trying to return object that was not rented: {typeof(T).Name}");
				return;
			}

			obj.gameObject.SetActive(false);
			obj.transform.SetParent(_parent);

			_activeObjects.Remove(obj);
			_inactiveObjects.Enqueue(obj);
		}

		public (int active, int inactive) GetStats()
		{
			return (_activeObjects.Count, _inactiveObjects.Count);
		}

		public void Clear()
		{
			foreach (var obj in _activeObjects)
			{
				if (obj != null)
					Object.Destroy(obj.gameObject);
			}

			foreach (var obj in _inactiveObjects)
			{
				if (obj != null)
					Object.Destroy(obj.gameObject);
			}

			_activeObjects.Clear();
			_inactiveObjects.Clear();
		}

		private T CreateNewObject()
		{
			var instance = Object.Instantiate(_prefab, _parent);
			var component = instance.GetComponent<T>();

			if (component == null)
			{
				_logger?.LogError($"[ObjectPool] Created object missing component: {typeof(T).Name}");
				Object.Destroy(instance);
				throw new InvalidOperationException($"Component {typeof(T).Name} not found on instantiated object");
			}

			return component;
		}
	}
}
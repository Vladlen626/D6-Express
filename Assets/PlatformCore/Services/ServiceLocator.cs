using System;
using System.Collections.Generic;

namespace PlatformCore.Core
{
	public sealed class ServiceLocator : IDisposable
	{
		private readonly Dictionary<Type, IService> _services = new();

		public void Register<TInterface, TImplementation>(TImplementation instance)
			where TInterface : class
			where TImplementation : class, IService, TInterface
		{
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));

			var key = typeof(TInterface);
			if (!_services.TryAdd(key, instance))
			{
				UnityEngine.Debug.LogWarning($"Service {key.Name} already registered. Skipped.");
			}
		}
		

		public T Get<T>() where T : class
		{
			if (_services.TryGetValue(typeof(T), out var service))
				return service as T;

			throw new InvalidOperationException($"Service {typeof(T).Name} not registered.");
		}

		public bool TryGet<T>(out T result) where T : class, IService
		{
			if (_services.TryGetValue(typeof(T), out var s))
			{
				result = (T)s;
				return true;
			}

			result = null;
			return false;
		}

		public IEnumerable<IService> All => new HashSet<IService>(_services.Values);

		public bool Has<T>() where T : class, IService
			=> _services.ContainsKey(typeof(T));

		public void Dispose()
		{
			foreach (var s in _services.Values)
			{
				try
				{
					s.Dispose();
				}
				catch (Exception e)
				{
					UnityEngine.Debug.LogError($"[ServiceLocator] Dispose failed for {s.GetType().Name}: {e}");
				}
			}

			_services.Clear();
		}
	}
}
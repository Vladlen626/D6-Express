using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace PlatformCore.Infrastructure.Lifecycle
{
	public class LifecycleService : IService
	{
		private readonly List<object> _managedObjects = new List<object>();
		private readonly List<IUpdatable> _updatables = new List<IUpdatable>();
		private readonly List<IFixedUpdatable> _fixedUpdatables = new List<IFixedUpdatable>();
		private readonly List<ILateUpdatable> _lateUpdatables = new List<ILateUpdatable>();
		
		public async UniTask RegisterAsync(object obj)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			_managedObjects.Add(obj);

			if (obj is IPreloadable preloadable)
			{
				await preloadable.PreloadAsync();
			}

			if (obj is IActivatable activatable)
			{
				activatable.Activate();
			}

			switch (obj)
			{
				case IUpdatable updatable:
					_updatables.Add(updatable);
					break;
				case IFixedUpdatable fixedUpdatable:
					_fixedUpdatables.Add(fixedUpdatable);
					break;
				case ILateUpdatable lateUpdatable:
					_lateUpdatables.Add(lateUpdatable);
					break;
			}
		}

		public void Unregister(object obj)
		{
			if (obj == null)
			{
				return;
			}

			if (_managedObjects.Contains(obj) == false)
			{
				return;
			}

			if (obj is IActivatable activatable)
			{
				activatable.Deactivate();
			}

			_managedObjects.Remove(obj);

			if (obj is IUpdatable updatable)
			{
				_updatables.Remove(updatable);
			}
			if (obj is IFixedUpdatable fixedUpdatable)
			{
				_fixedUpdatables.Remove(fixedUpdatable);
			}
			if (obj is ILateUpdatable lateUpdatable)
			{
				_lateUpdatables.Remove(lateUpdatable);
			}
		}

		public void Update(float deltaTime)
		{
			for (int i = _updatables.Count - 1; i >= 0; i--)
			{
				if (i < _updatables.Count)
				{
					_updatables[i]?.OnUpdate(deltaTime);
				}
			}
		}

		// ReSharper disable Unity.PerformanceAnalysis
		public void FixedUpdate(float fixedDeltaTime)
		{
			for (int i = _fixedUpdatables.Count - 1; i >= 0; i--)
			{
				if (i < _fixedUpdatables.Count)
				{
					_fixedUpdatables[i]?.OnFixedUpdate(fixedDeltaTime);
				}
			}
		}

		public void LateUpdate(float deltaTime)
		{
			for (int i = _lateUpdatables.Count - 1; i >= 0; i--)
			{
				if (i < _lateUpdatables.Count)
				{
					_lateUpdatables[i]?.OnLateUpdate(deltaTime);
				}
			}
		}

		public void Dispose()
		{
			for (int i = _managedObjects.Count - 1; i >= 0; i--)
			{
				var obj = _managedObjects[i];

				if (obj is IDeactivatable deactivatable)
				{
					deactivatable.Deactivate();
				}

				if (obj is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}

			_managedObjects.Clear();
			_updatables.Clear();
			_fixedUpdatables.Clear();
			_lateUpdatables.Clear();
		}
	}
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services;

namespace PlatformCore.Infrastructure.Lifecycle
{
	public class LifecycleService : IService
	{
		private readonly List<IBaseController> _managedObjects = new List<IBaseController>();
		private readonly List<IUpdatable> _updatables = new List<IUpdatable>();
		private readonly List<IFixedUpdatable> _fixedUpdatables = new List<IFixedUpdatable>();
		private readonly List<ILateUpdatable> _lateUpdatables = new List<ILateUpdatable>();

		public async UniTask RegisterAsync(IBaseController controller)
		{
			if (controller == null)
				throw new ArgumentNullException(nameof(controller));

			_managedObjects.Add(controller);

			if (controller is IPreloadable preloadable)
			{
				await preloadable.PreloadAsync();
			}

			if (controller is IActivatable activatable)
			{
				activatable.Activate();
			}

			switch (controller)
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

		public void Unregister(IBaseController controller)
		{
			if (controller == null)
			{
				return;
			}

			if (_managedObjects.Contains(controller) == false)
			{
				return;
			}

			if (controller is IActivatable activatable)
			{
				activatable.Deactivate();
			}

			_managedObjects.Remove(controller);


			switch (controller)
			{
				case IUpdatable updatable:
					_updatables.Remove(updatable);
					break;
				case IFixedUpdatable fixedUpdatable:
					_fixedUpdatables.Remove(fixedUpdatable);
					break;
				case ILateUpdatable lateUpdatable:
					_lateUpdatables.Remove(lateUpdatable);
					break;
			}
		}

		public async UniTask RegisterControllersGroupAsync(List<IBaseController> controllersList)
		{
			var tasks = new List<UniTask>();
			foreach (var controller in controllersList)
			{
				tasks.Add(RegisterAsync(controller));
			}

			await UniTask.WhenAll(tasks);
		}

		public async UniTask RegisterControllersGroupAsync(IBaseController[] controllersArray)
		{
			var tasks = new List<UniTask>();
			foreach (var controller in controllersArray)
			{
				tasks.Add(RegisterAsync(controller));
			}

			await UniTask.WhenAll(tasks);
		}

		public void UnregisterControllersGroup(List<IBaseController> controllersList)
		{
			foreach (var controller in controllersList)
			{
				Unregister(controller);
			}
		}

		public void UnregisterControllersGroup(IBaseController[] controllersArray)
		{
			foreach (var controller in controllersArray)
			{
				Unregister(controller);
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
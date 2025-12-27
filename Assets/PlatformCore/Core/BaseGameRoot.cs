using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

namespace PlatformCore.Core
{
	public abstract class BaseGameRoot : IDisposable
	{
#if UNITY_EDITOR
		public ServiceLocator EditorServices => _serviceLocator;
#endif
		protected ServiceLocator _serviceLocator { get; private set; }
		protected LifecycleService _lifecycle { get; private set; }

		private readonly ApplicationLifetimeService _lifetimeService;
		protected CancellationToken ApplicationCancellationToken => _lifetimeService.ApplicationLifetime;

		protected BaseGameRoot()
		{
			_serviceLocator = new ServiceLocator();
			Locator.Set(_serviceLocator);
			_lifecycle = new LifecycleService();
			_serviceLocator.Register<LifecycleService, LifecycleService>(_lifecycle);
			_lifetimeService = new ApplicationLifetimeService();
		}

		public async UniTask LaunchAsync(PersistentSceneContext context)
		{
			try
			{
				RegisterServices(context);
				await InitializeServicesAsync();
				await LaunchGameAsync(context);
			}
			catch (OperationCanceledException)
			{
				Debug.LogWarning("[GameRoot] Launch cancelled (application closing)");
			}
			catch (Exception ex)
			{
				Debug.LogError($"[GameRoot] Launch failed: {ex}");
				throw;
			}
		}

		protected virtual async UniTask InitializeServicesAsync()
		{
			await _serviceLocator.InitializeAllAsync(ApplicationCancellationToken);
		}

		protected abstract void RegisterServices(PersistentSceneContext persistentSceneContext);

		protected abstract UniTask LaunchGameAsync(PersistentSceneContext persistentSceneContext);


		public void OnUpdate(float delta)
		{
			_lifecycle.Update(delta);
		}

		public void OnFixedUpdate(float delta)
		{
			_lifecycle.FixedUpdate(delta);
		}

		public void OnLateUpdate(float delta)
		{
			_lifecycle.LateUpdate(delta);
		}

		public void Dispose()
		{
			try
			{
				Locator.Clear();
				_lifetimeService?.Dispose();
				_lifecycle?.Dispose();
				_serviceLocator?.Dispose();

				Debug.Log("[GameRoot] Disposed successfully");
			}
			catch (Exception ex)
			{
				Debug.LogError($"[GameRoot] Dispose error: {ex}");
			}
		}
	}
}
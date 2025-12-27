using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlatformCore.Services
{
	public class SceneService : BaseAsyncService, ISceneService
	{
		private readonly ILoggerService _loggerService;
		private Dictionary<string, AsyncOperation> _preloadedScenes = new();

		public SceneService(ILoggerService loggerService)
		{
			_loggerService = loggerService;
		}

		public async UniTask PreloadSceneAsync(string sceneName, CancellationToken ct = default)
		{
			var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			if (operation != null)
			{
				operation.allowSceneActivation = false;

				_preloadedScenes[sceneName] = operation;

				while (operation.progress < 0.9f)
				{
					await UniTask.Yield(cancellationToken: ct);
				}
			}
		}

		public async UniTask ActivatePreloadedScene(string sceneName)
		{
			if (_preloadedScenes.TryGetValue(sceneName, out var op))
			{
				op.allowSceneActivation = true;
				await op.ToUniTask();

				SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
				_preloadedScenes.Remove(sceneName);
			}
			else
			{
				throw new Exception($"Scene {sceneName} not preloaded!");
			}
		}

		public async UniTask LoadSceneAsync(string sceneName, CancellationToken ct = default)
		{
			await LoadSceneAsync(sceneName, LoadSceneMode.Additive, ct);
		}

		public UniTask LoadEnvironmentSceneAsync(string sceneName, bool unloadPrev, CancellationToken ct = default)
		{
			throw new NotImplementedException();
		}
		
		public async UniTask LoadSceneAsync(string sceneName, LoadSceneMode mode,
			CancellationToken ct = default)
		{
			_loggerService?.Log($"[SceneService] Loading scene: {sceneName} (mode: {mode})");

			if (string.IsNullOrEmpty(sceneName))
			{
				var error = "[SceneService] Scene name is null or empty";
				_loggerService?.LogError(error);
				throw new ArgumentException(error, nameof(sceneName));
			}

			try
			{
				var operation = SceneManager.LoadSceneAsync(sceneName, mode);

				if (operation == null)
				{
					throw new InvalidOperationException($"Failed to start loading scene: {sceneName}");
				}

				if (mode == LoadSceneMode.Single)
				{
					SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
				}
				
				
				await operation.ToUniTask(cancellationToken: ct);

				_loggerService?.Log($"[SceneService] Scene loaded successfully: {sceneName}");
			}
			catch (OperationCanceledException)
			{
				_loggerService?.Log($"[SceneService] Scene loading cancelled: {sceneName}");
				throw;
			}
			catch (Exception ex)
			{
				_loggerService?.LogError($"[SceneService] Failed to load scene: {sceneName}. Error: {ex.Message}");
				throw;
			}
		}

		public async UniTask ReloadCurrentSceneAsync(CancellationToken ct = default)
		{
			var currentSceneName = GetActiveSceneName();
			_loggerService?.Log($"[SceneService] Reloading current scene: {currentSceneName}");

			await LoadSceneAsync(currentSceneName, LoadSceneMode.Single, ct);
		}

		public async UniTask UnloadSceneAsync(string sceneName, CancellationToken ct = default)
		{
			_loggerService?.Log($"[SceneService] Unloading scene: {sceneName}");

			if (string.IsNullOrEmpty(sceneName))
			{
				_loggerService?.LogError("[SceneService] Scene name is null or empty");
				throw new ArgumentException("Scene name cannot be null or empty", nameof(sceneName));
			}

			if (!IsSceneLoaded(sceneName))
			{
				_loggerService?.LogWarning($"[SceneService] Scene not loaded, cannot unload: {sceneName}");
				return;
			}

			try
			{
				var operation = SceneManager.UnloadSceneAsync(sceneName);
				await operation.ToUniTask(cancellationToken: ct);

				_loggerService?.Log($"[SceneService] Scene unloaded successfully: {sceneName}");
			}
			catch (Exception ex)
			{
				_loggerService?.LogError($"[SceneService] Failed to unload scene: {sceneName}. Error: {ex.Message}");
				throw;
			}
		}

		public string GetActiveSceneName()
		{
			return SceneManager.GetActiveScene().name;
		}

		public bool TryGetSceneContext(string sceneName, out ISceneContext sceneContext)
		{
			sceneContext = null;

			var scene = SceneManager.GetSceneByName(sceneName);
			if (!scene.IsValid() || !scene.isLoaded) return false;

			var roots = scene.GetRootGameObjects();
			for (int i = 0; i < roots.Length; i++)
			{
				if (roots[i].TryGetComponent<ISceneContext>(out var ctx))
				{
					sceneContext = ctx;
					return true;
				}

				sceneContext = roots[i].GetComponentInChildren<ISceneContext>(true);
				if (sceneContext != null) return true;
			}

			return false;
		}

		public bool IsSceneLoaded(string sceneName)
		{
			if (string.IsNullOrEmpty(sceneName))
			{
				return false;
			}

			for (var i = 0; i < SceneManager.sceneCount; i++)
			{
				var scene = SceneManager.GetSceneAt(i);
				if (scene.name == sceneName && scene.isLoaded)
					return true;
			}

			return false;
		}
	}
}
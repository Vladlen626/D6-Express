using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlatformCore.Services.UI
{
	public class CursorService : ICursorService, IAsyncInitializable
	{
		public event Action OnCursorStateChanged;
		private readonly IUIService _uiService;
		private readonly ILoggerService _logger;

		private int lockCount;
		private UICursorView _uiCursorView;

		public bool IsCursorLocked => Cursor.lockState == CursorLockMode.Locked;

		public CursorService(IUIService uiService, ILoggerService logger)
		{
			_uiService = uiService;
			_logger = logger;
		}

		public UniTask PreInitializeAsync(CancellationToken ct)
		{
			return _uiService.PreloadAsync<UICursorView>();
		}

		public UniTask PostInitializeAsync(CancellationToken ct)
		{
			_uiCursorView = _uiService.GetWindow<UICursorView>();
			return UniTask.CompletedTask;
		}

		public void LockCursor()
		{
			var beforeCount = lockCount;
			var shouldLock = TryLock();
			var caller = GetCaller();

			if (shouldLock)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
				_uiCursorView.Show();
				OnCursorStateChanged?.Invoke();
				_logger?.Log($"[CursorService] LockCursor applied (caller={caller}, before={beforeCount}, after={lockCount}, lockState={Cursor.lockState})");
			}
			else
			{
				_logger?.Log($"[CursorService] LockCursor skipped (caller={caller}, before={beforeCount}, after={lockCount}, lockState={Cursor.lockState})");
			}
		}

		public void UnlockCursor()
		{
			var beforeCount = lockCount;
			var shouldUnlock = TryUnlock();
			var caller = GetCaller();

			if (shouldUnlock)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				_uiCursorView.Hide();
				OnCursorStateChanged?.Invoke();
				_logger?.Log($"[CursorService] UnlockCursor applied (caller={caller}, before={beforeCount}, after={lockCount}, lockState={Cursor.lockState})");
			}
			else
			{
				_logger?.Log($"[CursorService] UnlockCursor skipped (caller={caller}, before={beforeCount}, after={lockCount}, lockState={Cursor.lockState})");
			}
		}

		public void Dispose()
		{
			UnlockCursor();
		}

		private bool TryLock()
		{
			lockCount++;
			return lockCount == 1;
		}

		private bool TryUnlock()
		{
			if (lockCount == 0)
			{
				return false;
			}
			else
			{
				lockCount--;
				return lockCount == 0;
			}
		}

		private string GetCaller()
		{
			var frame = new StackFrame(2, false);
			var method = frame.GetMethod();
			if (method == null)
			{
				return "unknown";
			}

			var typeName = method.DeclaringType != null ? method.DeclaringType.Name : "unknown";
			return $"{typeName}.{method.Name}";
		}
	}
}

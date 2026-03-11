using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlatformCore.Services.UI
{
	public class CursorService : ICursorService, IAsyncInitializable
	{
		public event Action OnCursorStateChanged;
		private readonly IUIService _uiService;

		private int lockCount;
		private UICursorView _uiCursorView;

		public bool IsCursorLocked => Cursor.lockState == CursorLockMode.Locked;

		public CursorService(IUIService uiService)
		{
			_uiService = uiService;
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
			if (TryLock())
			{
				ApplyLockedState();
			}
		}

		public void UnlockCursor()
		{
			if (TryUnlock())
			{
				ApplyUnlockedState();
			}
		}



		public void ForceToggleCursor()
		{
			if (IsCursorLocked)
			{
				ForceUnlockCursor();
			}
			else
			{
				ForceLockCursor();
			}
		}

		public void Dispose()
		{
			UnlockCursor();
		}


		private void ForceLockCursor()
		{
			lockCount = 1;
			ApplyLockedState();
		}

		private void ForceUnlockCursor()
		{
			lockCount = 0;
			ApplyUnlockedState();
		}

		private void ApplyLockedState()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			_uiCursorView.Show();
			OnCursorStateChanged?.Invoke();
		}

		private void ApplyUnlockedState()
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			_uiCursorView.Hide();
			OnCursorStateChanged?.Invoke();
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
				return true;
			}
			else
			{
				lockCount--;
				return lockCount == 0;
			}
		}
	}
}

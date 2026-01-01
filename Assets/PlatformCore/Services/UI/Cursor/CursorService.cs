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
			LockCursor();
			return UniTask.CompletedTask;
		}

		public void LockCursor()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			_uiCursorView.Show();
			OnCursorStateChanged?.Invoke();
		}

		public void UnlockCursor()
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			_uiCursorView.Hide();
			OnCursorStateChanged?.Invoke();
		}

		public void Dispose()
		{
			UnlockCursor();
		}
	}
}
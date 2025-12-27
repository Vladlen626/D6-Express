using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using PlatformCore.Services.UI;
using Unity.VisualScripting;
using UnityEngine;

namespace PlatformCore.Infrastructure
{
	public class CursorService : ICursorService, ISyncInitializable
	{
		public event Action OnCursorStateChanged;
		private readonly IUIService _uiService;
		public bool IsCursorLocked => Cursor.lockState == CursorLockMode.Locked;

		public CursorService(IUIService uiService)
		{
			_uiService = uiService;
		}

		public void Initialize()
		{
			LockCursor();
		}

		public void LockCursor()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			OnCursorStateChanged?.Invoke();
		}

		public void UnlockCursor()
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			OnCursorStateChanged?.Invoke();
		}

		public void Dispose()
		{
			UnlockCursor();
		}
	}
}
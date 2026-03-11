using System;

namespace PlatformCore.Services.UI
{
	public interface ICursorService
	{
		public event Action OnCursorStateChanged;
		void LockCursor();
		void UnlockCursor();
		void ForceToggleCursor();
		bool IsCursorLocked { get; }
	}
}

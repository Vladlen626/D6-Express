using System;

namespace _Main.Scripts.UI
{
	public sealed class PauseState
	{
		public bool IsPaused { get; private set; }

		public event Action<bool> Changed;

		public void SetPaused(bool isPaused)
		{
			if (IsPaused == isPaused)
			{
				return;
			}

			IsPaused = isPaused;
			Changed?.Invoke(IsPaused);
		}
	}
}

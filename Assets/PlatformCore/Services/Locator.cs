namespace PlatformCore.Core
{
	public static class Locator
	{
		private static ServiceLocator _current;

		public static void Set(ServiceLocator locator)
		{
			_current = locator;
		}

		public static void Clear()
		{
			_current = null;
		}

		public static T Resolve<T>() where T : class
		{
			if (_current == null)
			{
				UnityEngine.Debug.LogError("[Locator] No active ServiceLocator set!");
				return null;
			}

			return _current.Get<T>();
		}
	}
}
using UnityEngine;

namespace _Main.Scripts.UI
{
	public static class TextStyleLibraryProvider
	{
		private const string DefaultResourcePath = "UI/TextStyleLibrary";
		private static TextStyleLibrary cached;

		public static TextStyleLibrary GetDefault()
		{
			if (cached)
			{
				return cached;
			}

			cached = Resources.Load<TextStyleLibrary>(DefaultResourcePath);
			return cached;
		}
	}
}

using System;
using UnityEngine;

namespace _Main.Scripts.UI
{
	[Serializable]
	public struct ColorStyleRef
	{
		[SerializeField]
		private string id;

		public string Id
		{
			get => id;
			set => id = value;
		}

		public Color Value
		{
			get
			{
				var library = ColorStyleLibraryProvider.GetDefault();
				if (library == null)
				{
					throw new InvalidOperationException("ColorStyleLibrary is missing.");
				}

				if (string.IsNullOrWhiteSpace(id))
				{
					throw new InvalidOperationException("Color style id is empty.");
				}

				var style = library.GetStyle(id);
				if (style == null)
				{
					throw new InvalidOperationException($"Color style '{id}' not found.");
				}

				return style.Color;
			}
		}

		public ColorStyleRef(string id)
		{
			this.id = id ?? string.Empty;
		}
	}
}

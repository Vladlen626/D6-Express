using System;
using TMPro;
using UnityEngine;

namespace _Main.Scripts.UI
{
	[Serializable]
	public struct TextStyleRef
	{
		[SerializeField]
		private string id;

		public string Id
		{
			get => id;
			set => id = value;
		}

		public TextStyleEntry Value
		{
			get
			{
				var library = TextStyleLibraryProvider.GetDefault();
				if (library == null)
				{
					throw new InvalidOperationException("TextStyleLibrary is missing.");
				}

				if (string.IsNullOrWhiteSpace(id))
				{
					throw new InvalidOperationException("Text style id is empty.");
				}

				var style = library.GetStyle(id);
				if (style == null)
				{
					throw new InvalidOperationException($"Text style '{id}' not found.");
				}

				return style;
			}
		}

		public Color Color => Value.Color;

		public void ApplyTo(TextMeshProUGUI text)
		{
			if (!text)
			{
				throw new InvalidOperationException("Text target is missing.");
			}

			var style = Value;
			text.color = style.Color;
			if (style.UseAdvanced)
			{
				if (style.FontSize > 0f)
				{
					text.fontSize = style.FontSize;
				}

				text.fontStyle = style.FontStyle;
			}
		}

		public TextStyleRef(string id)
		{
			this.id = id ?? string.Empty;
		}
	}
}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _Main.Scripts.UI
{
	[Serializable]
	public class TextStyleEntry
	{
		public string Id = "style";
		public Color Color = Color.white;
		public bool UseAdvanced = false;
		public float FontSize = 0f;
		public FontStyles FontStyle = FontStyles.Normal;
	}

	[CreateAssetMenu(menuName = "UI/Text Style Library", fileName = "TextStyleLibrary")]
	public class TextStyleLibrary : ScriptableObject
	{
		[SerializeField]
		private List<TextStyleEntry> styles = new();

		private readonly Dictionary<string, TextStyleEntry> styleMap = new(StringComparer.OrdinalIgnoreCase);
		private bool mapDirty = true;

		public IReadOnlyList<TextStyleEntry> Styles => styles;

		private void OnEnable()
		{
			RebuildMap();
		}

		private void OnValidate()
		{
			RebuildMap();
		}

		public TextStyleEntry GetStyle(string id)
		{
			if (styles == null || styles.Count == 0)
			{
				return null;
			}

			if (mapDirty)
			{
				RebuildMap();
			}

			if (string.IsNullOrWhiteSpace(id))
			{
				return styles[0];
			}

			var key = id.Trim();
			if (styleMap.TryGetValue(key, out var style) && style != null)
			{
				return style;
			}

			return styles[0];
		}

		public bool ContainsId(string id)
		{
			if (styles == null || styles.Count == 0 || string.IsNullOrWhiteSpace(id))
			{
				return false;
			}

			if (mapDirty)
			{
				RebuildMap();
			}

			return styleMap.ContainsKey(id.Trim());
		}

		public void MarkDirty()
		{
			mapDirty = true;
		}

		private void RebuildMap()
		{
			styleMap.Clear();
			if (styles == null)
			{
				mapDirty = false;
				return;
			}

			for (int i = 0; i < styles.Count; i++)
			{
				var style = styles[i];
				var id = style?.Id?.Trim();
				if (string.IsNullOrWhiteSpace(id))
				{
					continue;
				}

				styleMap[id] = style;
			}

			mapDirty = false;
		}
	}
}

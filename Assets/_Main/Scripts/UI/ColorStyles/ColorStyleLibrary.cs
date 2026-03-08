using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Main.Scripts.UI
{
	[Serializable]
	public class ColorStyleEntry
	{
		public string Id = "color";
		public Color Color = Color.white;
	}

	[CreateAssetMenu(menuName = "UI/Color Style Library", fileName = "ColorStyleLibrary")]
	public class ColorStyleLibrary : ScriptableObject
	{
		[SerializeField]
		private List<ColorStyleEntry> styles = new();

		private readonly Dictionary<string, ColorStyleEntry> styleMap = new(StringComparer.OrdinalIgnoreCase);
		private bool mapDirty = true;

		public IReadOnlyList<ColorStyleEntry> Styles => styles;

		private void OnEnable()
		{
			RebuildMap();
		}

		private void OnValidate()
		{
			RebuildMap();
		}

		public ColorStyleEntry GetStyle(string id)
		{
			if (styles == null || styles.Count == 0)
			{
				throw new InvalidOperationException("ColorStyleLibrary is empty.");
			}

			if (mapDirty)
			{
				RebuildMap();
			}

			if (string.IsNullOrWhiteSpace(id))
			{
				throw new InvalidOperationException("Color style id is empty.");
			}

			var key = id.Trim();
			if (styleMap.TryGetValue(key, out var style) && style != null)
			{
				return style;
			}

			throw new InvalidOperationException($"Color style '{id}' not found.");
		}

		public bool ContainsId(string id)
		{
			if (styles == null || styles.Count == 0)
			{
				return false;
			}

			if (mapDirty)
			{
				RebuildMap();
			}

			return !string.IsNullOrWhiteSpace(id) && styleMap.ContainsKey(id.Trim());
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

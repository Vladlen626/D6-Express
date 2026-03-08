using System;
using System.Collections.Generic;
using _Main.Scripts.UI;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace _Main.Scripts.Editor
{
	public class ColorStyleDropdown : AdvancedDropdown
	{
		private readonly ColorStyleLibrary library;
		private readonly Action<string> onSelected;
		private static readonly Dictionary<Color32, Texture2D> Swatches = new();

		public ColorStyleDropdown(AdvancedDropdownState state, ColorStyleLibrary library, Action<string> onSelected)
			: base(state)
		{
			this.library = library;
			this.onSelected = onSelected;
			minimumSize = new Vector2(220f, 280f);
		}

		protected override AdvancedDropdownItem BuildRoot()
		{
			var root = new AdvancedDropdownItem("Colors");

			if (library == null || library.Styles == null || library.Styles.Count == 0)
			{
				root.AddChild(new AdvancedDropdownItem("No colors"));
				return root;
			}

			for (int i = 0; i < library.Styles.Count; i++)
			{
				var style = library.Styles[i];
				if (style == null || string.IsNullOrWhiteSpace(style.Id))
				{
					continue;
				}

				var item = new ColorStyleDropdownItem(style.Id)
				{
					icon = GetSwatch(style.Color)
				};
				root.AddChild(item);
			}

			return root;
		}

		protected override void ItemSelected(AdvancedDropdownItem item)
		{
			if (item is ColorStyleDropdownItem styleItem)
			{
				onSelected?.Invoke(styleItem.Id);
			}
		}

		private Texture2D GetSwatch(Color color)
		{
			var key = (Color32)color;
			if (Swatches.TryGetValue(key, out var swatch) && swatch)
			{
				return swatch;
			}

			var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			texture.SetPixel(0, 0, color);
			texture.Apply();
			Swatches[key] = texture;
			return texture;
		}

		private sealed class ColorStyleDropdownItem : AdvancedDropdownItem
		{
			public string Id { get; }

			public ColorStyleDropdownItem(string id) : base(id)
			{
				Id = id;
			}
		}
	}
}

using _Main.Scripts.UI;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace _Main.Scripts.Editor
{
	public static class TextStyleEditorGUI
	{
		private const string MissingLibraryMessage = "TextStyleLibrary not found at Resources/UI/TextStyleLibrary.";

		public static void DrawStyleDropdown(string label, SerializedProperty styleIdProp, AdvancedDropdownState state)
		{
			var library = TextStyleLibraryProvider.GetDefault();
			if (library == null || library.Styles == null || library.Styles.Count == 0)
			{
				EditorGUILayout.PropertyField(styleIdProp, new GUIContent(label));
				EditorGUILayout.HelpBox(MissingLibraryMessage, MessageType.Info);
				return;
			}

			var rect = EditorGUILayout.GetControlRect();
			rect = EditorGUI.PrefixLabel(rect, new GUIContent(label));
			var display = string.IsNullOrWhiteSpace(styleIdProp.stringValue) ? "(none)" : styleIdProp.stringValue;
			if (EditorGUI.DropdownButton(rect, new GUIContent(display), FocusType.Passive))
			{
				var dropdown = new TextStyleDropdown(state, library, id =>
				{
					styleIdProp.stringValue = id;
					styleIdProp.serializedObject.ApplyModifiedProperties();
				});
				dropdown.Show(rect);
			}

			var style = library.GetStyle(styleIdProp.stringValue);
			if (style != null)
			{
				var colorRect = new Rect(rect.xMax - 18f, rect.y + 2f, 16f, rect.height - 4f);
				EditorGUI.DrawRect(colorRect, style.Color);
			}
		}
	}
}

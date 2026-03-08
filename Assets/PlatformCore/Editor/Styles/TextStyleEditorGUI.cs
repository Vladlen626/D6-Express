using _Main.Scripts.UI;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace _Main.Scripts.Editor
{
	public static class TextStyleEditorGUI
	{
		private const string MissingLibraryMessage = "TextStyleLibrary not found at Resources/UI/TextStyleLibrary.";

		public static void DrawStyleDropdown(Rect position, GUIContent label, SerializedProperty styleIdProp, AdvancedDropdownState state)
		{
			var library = TextStyleLibraryProvider.GetDefault();
			if (library == null || library.Styles == null || library.Styles.Count == 0)
			{
				EditorGUI.PropertyField(position, styleIdProp, label);
				return;
			}

			var buttonRect = EditorGUI.PrefixLabel(position, label);
			var display = string.IsNullOrWhiteSpace(styleIdProp.stringValue) ? "(none)" : styleIdProp.stringValue;
			if (EditorGUI.DropdownButton(buttonRect, new GUIContent(display), FocusType.Passive))
			{
				var dropdown = new TextStyleDropdown(state, library, id =>
				{
					styleIdProp.stringValue = id;
					styleIdProp.serializedObject.ApplyModifiedProperties();
				});
				dropdown.Show(buttonRect);
			}

			if (!string.IsNullOrWhiteSpace(styleIdProp.stringValue) && library.ContainsId(styleIdProp.stringValue))
			{
				var style = library.GetStyle(styleIdProp.stringValue);
				var colorRect = new Rect(buttonRect.xMax - 18f, buttonRect.y + 2f, 16f, buttonRect.height - 4f);
				EditorGUI.DrawRect(colorRect, style.Color);
			}
		}

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
			DrawStyleDropdown(rect, new GUIContent(label), styleIdProp, state);
		}
	}
}

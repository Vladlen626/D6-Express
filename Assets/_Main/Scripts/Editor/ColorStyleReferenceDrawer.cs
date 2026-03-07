using System.Collections.Generic;
using _Main.Scripts.UI;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace _Main.Scripts.Editor
{
	[CustomPropertyDrawer(typeof(ColorStyleRef))]
	public class ColorStyleRefDrawer : PropertyDrawer
	{
		private const string StyleIdFieldName = "id";
		private static readonly Dictionary<string, AdvancedDropdownState> StatesByPath = new();

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			var styleIdProp = property.FindPropertyRelative(StyleIdFieldName);
			if (styleIdProp == null)
			{
				EditorGUI.PropertyField(position, property, label, true);
				EditorGUI.EndProperty();
				return;
			}

			if (!StatesByPath.TryGetValue(property.propertyPath, out var state))
			{
				state = new AdvancedDropdownState();
				StatesByPath[property.propertyPath] = state;
			}

			ColorStyleEditorGUI.DrawStyleDropdown(position, label, styleIdProp, state);

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}
	}
}

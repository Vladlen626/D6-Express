using _Main.Scripts.UI;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace _Main.Scripts.Editor
{
	[CustomEditor(typeof(TextStyleApplier))]
	public class TextStyleApplierEditor : UnityEditor.Editor
	{
		private AdvancedDropdownState dropdownState;

		private void OnEnable()
		{
			dropdownState = new AdvancedDropdownState();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var styleIdProp = serializedObject.FindProperty("styleId");

			TextStyleEditorGUI.DrawStyleDropdown("Style", styleIdProp, dropdownState);

			EditorGUILayout.Space();
			DrawPropertiesExcluding(serializedObject, "m_Script", "styleId");

			serializedObject.ApplyModifiedProperties();
		}
	}
}

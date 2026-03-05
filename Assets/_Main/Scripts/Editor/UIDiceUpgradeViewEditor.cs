using _Main.Scripts.Dice;
using _Main.Scripts.UI;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace _Main.Scripts.Editor
{
	[CustomEditor(typeof(UIDiceUpgradeView))]
	public class UIDiceUpgradeViewEditor : UnityEditor.Editor
	{
		private AdvancedDropdownState dropdownState;

		private void OnEnable()
		{
			dropdownState = new AdvancedDropdownState();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var normalIdProp = serializedObject.FindProperty("normalValueStyleId");
			var changedIdProp = serializedObject.FindProperty("changedValueStyleId");

			TextStyleEditorGUI.DrawStyleDropdown("Normal Value Style", normalIdProp, dropdownState);
			TextStyleEditorGUI.DrawStyleDropdown("Changed Value Style", changedIdProp, dropdownState);

			EditorGUILayout.Space();
			DrawPropertiesExcluding(serializedObject, "m_Script", "normalValueStyleId", "changedValueStyleId");

			serializedObject.ApplyModifiedProperties();
		}
	}
}

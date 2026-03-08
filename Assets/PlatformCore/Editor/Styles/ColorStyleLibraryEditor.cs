using System.Collections.Generic;
using _Main.Scripts.UI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace _Main.Scripts.Editor
{
	[CustomEditor(typeof(ColorStyleLibrary))]
	public class ColorStyleLibraryEditor : UnityEditor.Editor
	{
		private ReorderableList stylesList;
		private SerializedProperty stylesProp;

		private void OnEnable()
		{
			stylesProp = serializedObject.FindProperty("styles");
			stylesList = new ReorderableList(serializedObject, stylesProp, true, true, true, true);
			stylesList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Color Styles");
			stylesList.elementHeightCallback = _ => EditorGUIUtility.singleLineHeight * 2f + 8f;
			stylesList.drawElementCallback = DrawElement;
			stylesList.onAddCallback = AddStyle;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			stylesList.DoLayoutList();
			DrawValidation();
			serializedObject.ApplyModifiedProperties();
		}

		private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			var element = stylesProp.GetArrayElementAtIndex(index);
			var idProp = element.FindPropertyRelative("Id");
			var colorProp = element.FindPropertyRelative("Color");

			float line = rect.y + 2f;
			var lineHeight = EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(new Rect(rect.x, line, rect.width, lineHeight), idProp, new GUIContent("Id"));
			line += lineHeight + 2f;
			EditorGUI.PropertyField(new Rect(rect.x, line, rect.width, lineHeight), colorProp, new GUIContent("Color"));
		}

		private void AddStyle(ReorderableList list)
		{
			stylesProp.arraySize++;
			var element = stylesProp.GetArrayElementAtIndex(stylesProp.arraySize - 1);
			var idProp = element.FindPropertyRelative("Id");
			var colorProp = element.FindPropertyRelative("Color");

			idProp.stringValue = GenerateUniqueId();
			colorProp.colorValue = Color.white;

			if (target is ColorStyleLibrary library)
			{
				library.MarkDirty();
			}
		}

		private string GenerateUniqueId()
		{
			var existing = new HashSet<string>();
			for (int i = 0; i < stylesProp.arraySize; i++)
			{
				var element = stylesProp.GetArrayElementAtIndex(i);
				var idProp = element.FindPropertyRelative("Id");
				if (!string.IsNullOrWhiteSpace(idProp.stringValue))
				{
					existing.Add(idProp.stringValue.Trim());
				}
			}

			int index = 1;
			string id;
			do
			{
				id = $"color_{index}";
				index++;
			} while (existing.Contains(id));

			return id;
		}

		private void DrawValidation()
		{
			var duplicates = new HashSet<string>();
			var seen = new HashSet<string>();
			for (int i = 0; i < stylesProp.arraySize; i++)
			{
				var element = stylesProp.GetArrayElementAtIndex(i);
				var idProp = element.FindPropertyRelative("Id");
				var id = idProp.stringValue?.Trim();
				if (string.IsNullOrWhiteSpace(id))
				{
					continue;
				}

				if (!seen.Add(id))
				{
					duplicates.Add(id);
				}
			}

			if (duplicates.Count > 0)
			{
				EditorGUILayout.HelpBox($"Duplicate color ids: {string.Join(", ", duplicates)}", MessageType.Warning);
			}
		}
	}
}

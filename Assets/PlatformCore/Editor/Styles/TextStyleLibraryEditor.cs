using System.Collections.Generic;
using _Main.Scripts.UI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace _Main.Scripts.Editor
{
	[CustomEditor(typeof(TextStyleLibrary))]
	public class TextStyleLibraryEditor : UnityEditor.Editor
	{
		private ReorderableList stylesList;
		private SerializedProperty stylesProp;

		private void OnEnable()
		{
			stylesProp = serializedObject.FindProperty("styles");
			stylesList = new ReorderableList(serializedObject, stylesProp, true, true, true, true);
			stylesList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Text Styles");
			stylesList.elementHeightCallback = GetElementHeight;
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
			var advancedProp = element.FindPropertyRelative("UseAdvanced");
			var sizeProp = element.FindPropertyRelative("FontSize");
			var styleProp = element.FindPropertyRelative("FontStyle");

			float line = rect.y + 2f;
			var lineHeight = EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(new Rect(rect.x, line, rect.width, lineHeight), idProp, new GUIContent("Id"));
			line += lineHeight + 2f;
			EditorGUI.PropertyField(new Rect(rect.x, line, rect.width, lineHeight), colorProp, new GUIContent("Color"));
			line += lineHeight + 2f;
			EditorGUI.PropertyField(new Rect(rect.x, line, rect.width, lineHeight), advancedProp, new GUIContent("Advanced"));
			if (advancedProp.boolValue)
			{
				line += lineHeight + 2f;
				EditorGUI.PropertyField(new Rect(rect.x, line, rect.width, lineHeight), styleProp, new GUIContent("Font Style"));
				line += lineHeight + 2f;
				EditorGUI.PropertyField(new Rect(rect.x, line, rect.width, lineHeight), sizeProp, new GUIContent("Size"));
			}
		}

		private float GetElementHeight(int index)
		{
			if (index < 0 || index >= stylesProp.arraySize)
			{
				return EditorGUIUtility.singleLineHeight * 3f + 12f;
			}

			var element = stylesProp.GetArrayElementAtIndex(index);
			var advancedProp = element.FindPropertyRelative("UseAdvanced");
			var lines = advancedProp.boolValue ? 5f : 3f;
			return EditorGUIUtility.singleLineHeight * lines + 12f;
		}

		private void AddStyle(ReorderableList list)
		{
			stylesProp.arraySize++;
			var element = stylesProp.GetArrayElementAtIndex(stylesProp.arraySize - 1);
			var idProp = element.FindPropertyRelative("Id");
			var colorProp = element.FindPropertyRelative("Color");
			var advancedProp = element.FindPropertyRelative("UseAdvanced");
			var sizeProp = element.FindPropertyRelative("FontSize");
			var styleProp = element.FindPropertyRelative("FontStyle");

			idProp.stringValue = GenerateUniqueId();
			colorProp.colorValue = Color.white;
			advancedProp.boolValue = false;
			sizeProp.floatValue = 0f;
			styleProp.intValue = (int)TMPro.FontStyles.Normal;

			if (target is TextStyleLibrary library)
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
				id = $"style_{index}";
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
				EditorGUILayout.HelpBox($"Duplicate style ids: {string.Join(", ", duplicates)}", MessageType.Warning);
			}
		}
	}
}

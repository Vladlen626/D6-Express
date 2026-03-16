using _Main.Scripts.Dice;
using UnityEditor;
using UnityEngine;

namespace _Main.Scripts.Editor
{
	[CustomEditor(typeof(ItemView))]
	public class ItemViewEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			EditorGUILayout.Space();
			if (!GUILayout.Button("Auto Configure Disabled Renderers"))
			{
				return;
			}

			for (int i = 0; i < targets.Length; i++)
			{
				if (targets[i] is not ItemView view)
				{
					continue;
				}

				Undo.RecordObject(view, "Auto Configure Disabled Renderers");
				view.AutoConfigureDisabledRenderers();
				EditorUtility.SetDirty(view);
				PrefabUtility.RecordPrefabInstancePropertyModifications(view);
			}
		}
	}
}

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using PlatformCore.Services.UI;

[CustomEditor(typeof(UIBackgroundSizer))]
public class UIBackgroundSizerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if (GUILayout.Button("Apply"))
		{
			var sizer = (UIBackgroundSizer)target;
			sizer.RefreshInEditor();
			EditorUtility.SetDirty(sizer);
		}
	}
}
#endif

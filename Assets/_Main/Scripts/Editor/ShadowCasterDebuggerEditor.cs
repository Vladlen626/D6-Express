using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShadowCasterDebugger))]
public class ShadowCasterDebuggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ShadowCasterDebugger debugger = (ShadowCasterDebugger)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("Log Shadow Casters"))
        {
            debugger.LogShadowCasters();
        }

        if (GUILayout.Button("Disable Shadows On Last Matched"))
        {
            debugger.DisableShadowsOnLastMatched();
        }
    }
}

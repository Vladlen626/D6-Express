using UnityEditor;

public class EditorWindowDebugVariables : EditorWindow
{
    [MenuItem("Tools/Debug Variables")]
    static void Open() => GetWindow<EditorWindowDebugVariables>("Debug Variables");

    void OnGUI()
    {
        DebugVariables.ShowLoseView = EditorGUILayout.Toggle("Show Lose View", DebugVariables.ShowLoseView);
        DebugVariables.ShowWinView = EditorGUILayout.Toggle("Show Win View", DebugVariables.ShowWinView);

        DebugVariables.StartSpawnLocation = (Location)EditorGUILayout.EnumPopup("Start Spawn Location", DebugVariables.StartSpawnLocation);
    }
}
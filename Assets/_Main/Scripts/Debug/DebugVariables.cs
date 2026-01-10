using UnityEditor;

public static class DebugVariables
{
    private static readonly string ShowLoseKey = "DebugVars_ShowLoseView";
    private static readonly string ShowWinKey = "DebugVars_ShowWinView";
    private static readonly string SpawnLocKey = "DebugVars_StartSpawnLocation";

    public static bool ShowLoseView {
        get => EditorPrefs.GetBool(ShowLoseKey, false);
        set => EditorPrefs.SetBool(ShowLoseKey, value);
    }
    
    public static bool ShowWinView {
        get => EditorPrefs.GetBool(ShowWinKey, false);
        set => EditorPrefs.SetBool(ShowWinKey, value);
    }
    
    public static LevelState StartSpawnLocation {
        get => (LevelState)EditorPrefs.GetInt(SpawnLocKey, 0);
        set => EditorPrefs.SetInt(SpawnLocKey, (int)value);
    }
}
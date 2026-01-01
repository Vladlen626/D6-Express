using UnityEditor;
using UnityEditor.SceneManagement;

public static class GeneratedScenesMenu
{

    [MenuItem("Scenes/Persistent", false, 1)]
    private static void Open_Persistent()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Persistent.unity");
        }
    }

    [MenuItem("Scenes/Train", false, 2)]
    private static void Open_Train()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Train.unity");
        }
    }

    [MenuItem("Scenes/MainMenu", false, 3)]
    private static void Open_MainMenu()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        }
    }
}

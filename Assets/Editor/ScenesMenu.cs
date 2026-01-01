using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

public static class ScenesMenuGenerator
{
	private const string GeneratedFilePath = "Assets/Editor/GeneratedScenesMenu.cs";

	[MenuItem("Scenes/Refresh", false, 0)]
	private static void Generate()
	{
		var scenes = EditorBuildSettings.scenes;
		StringBuilder sb = new StringBuilder();

		sb.AppendLine("using UnityEditor;");
		sb.AppendLine("using UnityEditor.SceneManagement;");
		sb.AppendLine("");
		sb.AppendLine("public static class GeneratedScenesMenu");
		sb.AppendLine("{");

		for (int i = 0; i < scenes.Length; i++)
		{
			string path = scenes[i].path;
			string name = Path.GetFileNameWithoutExtension(path);
			string safeName = name.Replace(" ", "_");

			sb.AppendLine(
				$@"
    [MenuItem(""Scenes/{name}"", false, {i + 1})]
    private static void Open_{safeName}()
    {{
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {{
            EditorSceneManager.OpenScene(""{path}"");
        }}
    }}"
			);
		}

		sb.AppendLine("}");

		File.WriteAllText(GeneratedFilePath, sb.ToString());
		AssetDatabase.Refresh();
	}
}
using UnityEditor;
using System.IO;
using System.Text;
using FMODUnity;
using UnityEngine;

public static class FMODSoundNamesGenerator
{
	[MenuItem("Tools/Generate SoundNames")]
	public static void Generate()
	{
		var events = EventManager.Events;

		StringBuilder sb = new StringBuilder();
		sb.AppendLine("public static class SoundNames");
		sb.AppendLine("{");

		int count = 0;

		foreach (var kv in events)
		{
			string path = kv.Path;
			string fieldName = PathToFieldName(path);

			sb.AppendLine($"    public const string {fieldName} = \"{path}\";");
			count++;
		}

		sb.AppendLine("}");

		string filePath = "Assets/_Main/Scripts/Audio/SoundNames.cs";
		Directory.CreateDirectory(Path.GetDirectoryName(filePath));
		File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

		AssetDatabase.Refresh();

		Debug.Log($"SoundNames generated. Events: {count}. File: {filePath}");
	}

	static string PathToFieldName(string path)
	{
		// event:/UI/DiceClick -> DiceClick
		int lastSlashIndex = path.LastIndexOf('/');
		if (lastSlashIndex >= 0)
		{
			return path.Substring(lastSlashIndex + 1);
		}

		return path;
	}
}
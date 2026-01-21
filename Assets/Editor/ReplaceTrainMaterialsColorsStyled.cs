using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ReplaceTrainMaterialsColorsMuted
{
	private const string TargetFolder = "Assets/_Main/Materials/TrainMats";

	// Материалы и тусклые цвета для них
	private static readonly Dictionary<string, string> MaterialToHex = new Dictionary<string, string>
	{
		{ "Battery", "#6E576E" },
		{ "Crane", "#3C3444" },
		{ "Train_Frame", "#c5b7cb" },

		{ "Floor", "#3c3444" },
		{ "Tabletop", "#C5B7CB" },
		{ "Titan_Back_Wall", "#0E131E" },
		
		{ "Watch", "#0d0305" },

		{ "Plastic", "#f9cf9d" },
		{ "Plastic_dark", "#f9cf9d" },
		{ "Paper", "#f7f4e8" },

		{ "Кран синий", "#2a69b0" },
		{ "Пластик туалет", "#f9cf9d" },

		{ "Поручни", "#851246" },
		{ "Резина", "#0e131e" },
		{ "Черный", "#0D0305" }, 
		{ "Shelf", "#851246" },
		{ "Metal", "#c5b7cb" },
		{ "Штора ткань", "#c5b7cb" }
	};

	[MenuItem("Tools/Replace Train Materials Colors Muted")]
	public static void ReplaceColorsMuted()
	{
		Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
		if (litShader == null)
		{
			Debug.LogError("URP/Lit shader not found");
			return;
		}

		foreach (var pair in MaterialToHex)
		{
			string matName = pair.Key;
			string hex = pair.Value;

			if (!ColorUtility.TryParseHtmlString(hex, out Color color))
			{
				Debug.LogWarning($"Invalid hex color: {hex}");
				continue;
			}

			string[] guids = AssetDatabase.FindAssets(matName + " t:Material", new[] { TargetFolder });
			if (guids.Length == 0)
			{
				Debug.LogWarning($"Material '{matName}' not found in {TargetFolder}");
				continue;
			}

			foreach (string guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
				if (mat == null) continue;

				mat.SetColor("_BaseColor", color);
				EditorUtility.SetDirty(mat);
			}
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		Debug.Log("Colors replaced on Train materials (muted style)");
	}
}
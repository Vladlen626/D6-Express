using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ReplaceTrainMaterialsColorsMuted
{
	private const string TargetFolder = "Assets/_Main/Materials/TrainMats";

	// Материалы и тусклые цвета для них
	private static readonly Dictionary<string, string> MaterialToHex = new Dictionary<string, string>
	{
		{ "Battery", "#7c7086" },
		{ "Crane", "#eb5350" },
		{ "Train_Frame", "#38543f" },

		{ "Floor", "#3d3b59" },
		{ "Tabletop", "#b9a292" },
		{ "Titan_Back_Wall", "#7c7086" },
		
		{ "Watch", "#21110a" },

		{ "Plastic", "#b9a292" },
		{ "Plastic_dark", "#b9a292" },
		{ "Paper", "#fbdfc7" },

		{ "Кран синий", "#3d3b59" },
		{ "Пластик туалет", "#fbdfc7" },

		{ "Поручни", "#ee9662" },
		{ "Резина", "#21110a" },
		{ "Черный", "#21110a" }, 
		{ "Shelf", "#eb5350" },
		{ "Metal", "#7c7086" },
		{ "Штора ткань", "#fdc68b" }
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
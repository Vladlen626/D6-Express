using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class CreateMaterialsFromHex
{
	private const string PaletteName = "Kirb32";
	private const string RootFolder = "Assets/_Main/Materials";

	private static readonly string[] HexColors =
	{
		"#0D0305",
		"#3C3444",
		"#6E576E",
		"#917D9B",
		"#C5B7CB",
		"#F7F4E8",

		"#5F4F47",
		"#851246",
		"#D72048",
		"#7D322F",
		"#9D4C2F",
		"#C65E2D",
		"#F96A2D",
		"#FFA300",
		"#E29138",
		"#F7C233",
		"#F9EC41",

		"#11442C",
		"#287A33",
		"#52B139",
		"#8AE931",

		"#0E131E",
		"#203C62",
		"#2A69B0",
		"#00A1DE",
		"#6BDAD5",

		"#A52EB8",
		"#F7406E",
		"#FC83A2",
		"#F9CF9D",
		"#FBA176",
		"#F66F67"
	};

	private static readonly HashSet<string> BackgroundColors = new()
	{
		"#0D0305", "#3C3444", "#6E576E", "#917D9B",
		"#5F4F47", "#7D322F", "#9D4C2F",
		"#11442C", "#287A33",
		"#0E131E", "#203C62"
	};

	private static readonly HashSet<string> AccentColors = new()
	{
		"#851246", "#D72048", "#C65E2D", "#F96A2D",
		"#FFA300", "#E29138", "#F7C233",
		"#2A69B0", "#00A1DE", "#6BDAD5",
		"#A52EB8", "#F7406E", "#F66F67"
	};

	[MenuItem("Tools/Create Materials From Hex (Palette folders)")]
	public static void CreateMaterials()
	{
		CreateFolderIfMissing("Assets", "_Main");
		CreateFolderIfMissing("Assets/_Main", "Materials");

		string palettePath = $"{RootFolder}/{PaletteName}";
		CreateFolderIfMissing(RootFolder, PaletteName);

		string bgPath = $"{palettePath}/_Background";
		string accentPath = $"{palettePath}/_Accent";
		string fallbackPath = $"{palettePath}/_Fallback";

		CreateFolderIfMissing(palettePath, "_Background");
		CreateFolderIfMissing(palettePath, "_Accent");
		CreateFolderIfMissing(palettePath, "_Fallback");

		Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
		if (litShader == null)
		{
			Debug.LogError("URP/Lit shader not found");
			return;
		}

		foreach (string hex in HexColors)
		{
			if (!ColorUtility.TryParseHtmlString(hex, out Color color))
			{
				Debug.LogWarning($"Invalid hex color: {hex}");
				continue;
			}

			string targetFolder = GetTargetFolder(hex, bgPath, accentPath, fallbackPath);
			string cleanName = hex.Replace("#", "");
			string materialPath = $"{targetFolder}/{cleanName}.mat";

			if (File.Exists(materialPath))
			{
				continue;
			}

			Material mat = new Material(litShader);
			mat.SetColor("_BaseColor", color);

			AssetDatabase.CreateAsset(mat, materialPath);
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Debug.Log($"Palette '{PaletteName}' materials created");
	}

	private static string GetTargetFolder(
		string hex,
		string bg,
		string accent,
		string fallback
	)
	{
		if (BackgroundColors.Contains(hex))
		{
			return bg;
		}

		if (AccentColors.Contains(hex))
		{
			return accent;
		}

		return fallback;
	}

	private static void CreateFolderIfMissing(string parent, string name)
	{
		if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
		{
			AssetDatabase.CreateFolder(parent, name);
		}
	}
}
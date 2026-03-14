using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class UIButtonClickSoundEditorTool
{
	private const string MenuPath = "Assets/Audio/Add Button Click Sound To Buttons";

	[MenuItem(MenuPath, false)]
	private static void AddClickSoundToSelectedPrefabs()
	{
		var prefabPaths = CollectSelectedPrefabPaths();
		if (prefabPaths.Count == 0)
		{
			Debug.LogWarning("[UIButtonClickSoundTool] No prefabs found in the current selection.");
			return;
		}

		var prefabsProcessed = 0;
		var buttonsFound = 0;
		var componentsAdded = 0;
		var failedPrefabs = 0;
		var pathList = new List<string>(prefabPaths);
		pathList.Sort(StringComparer.OrdinalIgnoreCase);

		try
		{
			for (int i = 0; i < pathList.Count; i++)
			{
				var prefabPath = pathList[i];
				EditorUtility.DisplayProgressBar(
					"Add Button Click Sound",
					prefabPath,
					(float)i / pathList.Count);

				if (!ProcessPrefab(prefabPath, ref buttonsFound, ref componentsAdded))
				{
					failedPrefabs++;
					continue;
				}

				prefabsProcessed++;
			}
		}
		finally
		{
			EditorUtility.ClearProgressBar();
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Debug.Log(
			$"[UIButtonClickSoundTool] Done. Prefabs processed: {prefabsProcessed}. Buttons found: {buttonsFound}. Components added: {componentsAdded}. Failed prefabs: {failedPrefabs}.");
	}

	[MenuItem(MenuPath, true)]
	private static bool ValidateAddClickSoundToSelectedPrefabs()
	{
		return HasPrefabOrFolderSelection();
	}

	private static bool ProcessPrefab(string prefabPath, ref int buttonsFound, ref int componentsAdded)
	{
		GameObject root = null;
		try
		{
			root = PrefabUtility.LoadPrefabContents(prefabPath);
			if (!root)
			{
				Debug.LogError($"[UIButtonClickSoundTool] Failed to load prefab: {prefabPath}");
				return false;
			}

			var buttons = root.GetComponentsInChildren<Button>(true);
			buttonsFound += buttons.Length;

			var isDirty = false;
			for (int i = 0; i < buttons.Length; i++)
			{
				var button = buttons[i];
				if (!button)
				{
					continue;
				}

				if (button.GetComponent<UIButtonClickSound>())
				{
					continue;
				}

				button.gameObject.AddComponent<UIButtonClickSound>();
				componentsAdded++;
				isDirty = true;
			}

			if (isDirty)
			{
				PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
			}

			return true;
		}
		catch (Exception e)
		{
			Debug.LogError($"[UIButtonClickSoundTool] Failed processing prefab '{prefabPath}': {e}");
			return false;
		}
		finally
		{
			if (root)
			{
				PrefabUtility.UnloadPrefabContents(root);
			}
		}
	}

	private static HashSet<string> CollectSelectedPrefabPaths()
	{
		var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		var guids = Selection.assetGUIDs;
		for (int i = 0; i < guids.Length; i++)
		{
			var path = AssetDatabase.GUIDToAssetPath(guids[i]);
			if (string.IsNullOrEmpty(path))
			{
				continue;
			}

			if (AssetDatabase.IsValidFolder(path))
			{
				var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { path });
				for (int j = 0; j < prefabGuids.Length; j++)
				{
					var prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[j]);
					if (!string.IsNullOrEmpty(prefabPath))
					{
						result.Add(prefabPath);
					}
				}
				continue;
			}

			if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
			{
				result.Add(path);
			}
		}

		return result;
	}

	private static bool HasPrefabOrFolderSelection()
	{
		var guids = Selection.assetGUIDs;
		if (guids == null || guids.Length == 0)
		{
			return false;
		}

		for (int i = 0; i < guids.Length; i++)
		{
			var path = AssetDatabase.GUIDToAssetPath(guids[i]);
			if (string.IsNullOrEmpty(path))
			{
				continue;
			}

			if (AssetDatabase.IsValidFolder(path))
			{
				return true;
			}

			if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}
}

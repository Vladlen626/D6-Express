using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PersistentPlayToggle
{
	private const string PersistentScenePath = "Assets/Scenes/Persistent.unity";
	private const string PrefKey = "Scenes_UsePersistent";

	static PersistentPlayToggle()
	{
		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		EditorApplication.delayCall += SyncState;
	}

	[MenuItem("Scenes/Use Persistent", false, 0)]
	private static void Toggle()
	{
		bool enabled = EditorPrefs.GetBool(PrefKey, false);
		enabled = !enabled;

		EditorPrefs.SetBool(PrefKey, enabled);
		Apply(enabled);
	}

	[MenuItem("Scenes/Use Persistent", true)]
	private static bool ToggleValidate()
	{
		Menu.SetChecked(
			"Scenes/Use Persistent",
			EditorPrefs.GetBool(PrefKey, false)
		);
		return true;
	}

	private static void OnPlayModeStateChanged(PlayModeStateChange state)
	{
		if (state != PlayModeStateChange.ExitingEditMode)
		{
			return;
		}

		Apply(EditorPrefs.GetBool(PrefKey, false));
	}

	private static void Apply(bool enabled)
	{
		if (enabled)
		{
			SceneAsset persistentScene =
				AssetDatabase.LoadAssetAtPath<SceneAsset>(PersistentScenePath);

			if (persistentScene == null)
			{
				Debug.LogError("Persistent scene not found: " + PersistentScenePath);
				return;
			}

			EditorSceneManager.playModeStartScene = persistentScene;
		}
		else
		{
			EditorSceneManager.playModeStartScene = null;
		}
	}

	private static void SyncState()
	{
		Apply(EditorPrefs.GetBool(PrefKey, false));
	}
}
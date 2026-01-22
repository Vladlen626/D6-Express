using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Lightweight on-screen debug overlay to show the current scrambled score map.
	/// Lives in the modifiers plugin so we avoid touching UI code elsewhere.
	/// </summary>
	public class ScrambleCombinationsOverlay : MonoBehaviour
	{
		private static ScrambleCombinationsOverlay instance;
		private string mapText;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Bootstrap()
		{
			EnsureInstance();
		}

		public static void UpdateMap(Dictionary<DiceCombination, int> scrambledScores)
		{
			EnsureInstance();

			var sb = new StringBuilder();
			sb.AppendLine("Scrambled score map:");
			foreach (var pair in scrambledScores)
			{
				sb.AppendLine($"{DiceGameUtils.GetCombinationName(pair.Key)} -> {pair.Value}");
			}

			instance.mapText = sb.ToString();
		}

		private static void EnsureInstance()
		{
			if (instance != null)
			{
				return;
			}

			var go = new GameObject("ScrambleCombinationsOverlay");
			DontDestroyOnLoad(go);
			instance = go.AddComponent<ScrambleCombinationsOverlay>();
		}

		private void OnGUI()
		{
			if (string.IsNullOrEmpty(mapText))
			{
				return;
			}

			var areaRect = new Rect(Screen.width - 280, 10, 260, Screen.height / 2f);
			var style = new GUIStyle(GUI.skin.label)
			{
				fontSize = 14,
				alignment = TextAnchor.UpperLeft,
				wordWrap = true
			};

			GUILayout.BeginArea(areaRect, GUIContent.none, GUI.skin.box);
			GUILayout.Label(mapText, style);
			GUILayout.EndArea();
		}
	}
}

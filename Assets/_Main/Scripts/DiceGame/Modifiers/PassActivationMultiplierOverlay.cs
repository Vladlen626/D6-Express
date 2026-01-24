using System;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Simple top-screen button for arming the pass score multiplier. Lives entirely inside the modifiers plugin.
	/// </summary>
	public class PassActivationMultiplierOverlay : MonoBehaviour
	{
		private static PassActivationMultiplierOverlay instance;
		private static Action activateCallback;

		private int remaining;
		private bool isArmed;
		private bool isVisible;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Bootstrap()
		{
			EnsureInstance();
		}

		public static void RegisterActivateCallback(Action onClicked)
		{
			EnsureInstance();
			activateCallback = onClicked;
		}

		public static void UpdateState(int remainingUses, bool armed, bool visible)
		{
			EnsureInstance();
			instance.remaining = remainingUses;
			instance.isArmed = armed;
			instance.isVisible = visible;
		}

		private static void EnsureInstance()
		{
			if (instance != null)
			{
				return;
			}

			var go = new GameObject("PassActivationMultiplierOverlay");
			DontDestroyOnLoad(go);
			instance = go.AddComponent<PassActivationMultiplierOverlay>();
		}

		private void OnGUI()
		{
			if (!isVisible)
			{
				return;
			}

			var label = isArmed
				? "Pass x1.5: ARMED"
				: $"Pass x1.5: {remaining} left";

			var rect = new Rect(Screen.width * 0.5f - 90f, 10f, 180f, 32f);

			GUI.enabled = !isArmed && remaining > 0;
			if (GUI.Button(rect, label))
			{
				activateCallback?.Invoke();
			}
			GUI.enabled = true;
		}
	}
}

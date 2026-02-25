using DG.Tweening;
using UnityEngine;

namespace _Main.Scripts.Core
{
	public static class GlobalParameters
	{
		private static float animSpeed = 1f;

		public static float AnimSpeed
		{
			get => animSpeed;
			set
			{
				animSpeed = Mathf.Max(0.01f, value);
				DOTween.timeScale = animSpeed;
			}
		}

		private static int delay = 300;
		public static int Delay
		{
			get => (int)(delay / animSpeed);
		
			set => delay = value;
		}

		private static float enemyTurnDelayMultiplier = 1.5f;
		public static float EnemyTurnDelayMultiplier
		{
			get => enemyTurnDelayMultiplier;
			set => enemyTurnDelayMultiplier = Mathf.Max(0.1f, value);
		}
	}
}

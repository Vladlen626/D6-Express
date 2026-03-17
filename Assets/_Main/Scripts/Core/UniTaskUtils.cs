using Cysharp.Threading.Tasks;
using System;
using DG.Tweening;

namespace _Main.Scripts
{
	public static class UniTaskUtils
	{
		// ReSharper disable Unity.PerformanceAnalysis
		public static UniTask WaitAllTweens(params Tween[] tweens)
		{
			return WaitAllTweens(tweens, tweens?.Length ?? 0);
		}

		// ReSharper disable Unity.PerformanceAnalysis
		public static UniTask WaitAllTweens(Tween[] tweens, int count)
		{
			if (tweens == null || count <= 0)
			{
				return UniTask.CompletedTask;
			}

			if (count > tweens.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(count), count, "Tween count cannot be greater than array length.");
			}

			var sequence = DOTween.Sequence();

			for (int i = 0; i < count; i++)
			{
				var tween = tweens[i];
				if (tween == null || !tween.active)
				{
					continue;
				}

				sequence.Join(tween);
			}

			if (sequence.active == false || sequence.Duration() <= 0f)
			{
				sequence.Kill();
				return UniTask.CompletedTask;
			}

			return sequence.AsyncWaitForCompletion().AsUniTask();
		}
	}
}

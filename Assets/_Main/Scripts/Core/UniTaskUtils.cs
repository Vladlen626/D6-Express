using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace _Main.Scripts
{
	public static class UniTaskUtils
	{
		// ReSharper disable Unity.PerformanceAnalysis
		public static UniTask WaitAllTweens(params Tween[] tweens)
		{
			if (tweens == null || tweens.Length == 0)
			{
				return UniTask.CompletedTask;
			}

			var sequence = DOTween.Sequence();

			for (int i = 0; i < tweens.Length; i++)
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
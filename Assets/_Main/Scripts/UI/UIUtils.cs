using DG.Tweening;
using TMPro;

namespace _Main.Scripts.UI
{
	public static class UIUtils
	{
		public static void UpdateUiIntValueText(TMP_Text tmp, int from, int to, System.Func<int, string> formatter)
		{
			int value = from;

			DOTween.To(() => value, x => value = x, to, 0.25f)
				.SetEase(Ease.Linear)
				.OnUpdate(() =>
				{
					tmp.text = formatter(value);
				});
		}
	}
}
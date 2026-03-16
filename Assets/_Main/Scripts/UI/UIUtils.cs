using DG.Tweening;
using TMPro;

namespace _Main.Scripts.UI
{
	public static class UIUtils
	{
		public static void UpdateUiIntValueText(TMP_Text tmp, int from, int to, string pattern, float duration = 0.25f)
		{
			if (!tmp)
			{
				return;
			}

			int value = from;
			int lastRenderedValue = int.MinValue;

			DOTween.To(() => value, x => value = x, to, duration)
				.SetEase(Ease.Linear)
				.OnUpdate(() =>
				{
					if (!tmp || lastRenderedValue == value)
					{
						return;
					}

					lastRenderedValue = value;
					tmp.SetText(pattern, value);
				});
		}
	}
}
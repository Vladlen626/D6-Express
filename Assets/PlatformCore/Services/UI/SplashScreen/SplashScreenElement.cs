using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace PlatformCore.Services.UI.SplashScreen
{
	public class SplashScreenElement : BaseUIElement
	{
		[SerializeField]
		private CanvasGroup _canvasGroup;
		public void OnShow(float duration)
		{
			_canvasGroup.DOFade(1, duration);
		}

		public void OnHide(float duration)
		{
			_canvasGroup.DOFade(0, duration);
		}
	}
}
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlatformCore.Services.UI;
using UnityEngine;

public class UITransitionView : UIBaseElement
{
	[SerializeField]
	private RectTransform toScale;

	[SerializeField]
	private CanvasGroup loadingTextGroup;

	public UniTask ShowAsync(float duration)
	{
		return UniTask.WhenAll(
			loadingTextGroup.DOFade(1, duration * 0.8f).AsyncWaitForCompletion().AsUniTask(),
			toScale.DOSizeDelta(Vector2.zero, duration).SetEase(Ease.InOutBack).AsyncWaitForCompletion().AsUniTask()
		);
	}

	public UniTask HideAsync(float duration)
	{
		toScale.sizeDelta = Vector2.zero;

		return UniTask.WhenAll(
			loadingTextGroup.DOFade(0, duration * 0.8f).AsyncWaitForCompletion().AsUniTask(),
			toScale.DOSizeDelta(GetExpandedSize(), duration).SetEase(Ease.OutBack).AsyncWaitForCompletion().AsUniTask()
		);
	}

	private Vector2 GetExpandedSize()
	{
		var parentRect = toScale.parent as RectTransform;
		Vector2 baseSize;

		if (parentRect)
		{
			baseSize = parentRect.rect.size;
		}
		else
		{
			baseSize = new Vector2(Screen.width, Screen.height);
		}

		float side = Mathf.Sqrt(baseSize.x * baseSize.x + baseSize.y * baseSize.y);
		return new Vector2(side, side);
	}
}

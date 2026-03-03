using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

public class UITransitionView : UIBaseElement
{
	[SerializeField] private RectTransform upper;

	[SerializeField] private RectTransform bottom;

	[SerializeField] private AnimationCurve curve;

	[SerializeField] private CanvasGroup stationNameCanvasGroup;

	[SerializeField] private TextMeshProUGUI locationName;

	[SerializeField] private CanvasGroup hintGroup;

	[SerializeField] private UIModifiersView uIModifiersView;

	private float initialUpperY, initialBottomY;

	public UIModifiersView UIModifiersView => uIModifiersView;

	private void Start()
	{
		initialUpperY = upper.anchoredPosition.y;
		initialBottomY = bottom.anchoredPosition.y;
	}

	public async UniTask ShowAsync(float duration)
	{
		Show();

		var upperMove = upper.DOAnchorPosY(0f, duration).SetEase(curve).AsyncWaitForCompletion().AsUniTask();
		var bottomMove = bottom.DOAnchorPosY(0f, duration).SetEase(curve).AsyncWaitForCompletion().AsUniTask();

		await UniTask.WhenAll(upperMove, bottomMove);
	}

	public async UniTask HideAsync(float duration)
	{
		var upperMove = upper.DOAnchorPosY(initialUpperY, duration).SetEase(curve).AsyncWaitForCompletion().AsUniTask();
		var bottomMove = bottom.DOAnchorPosY(initialBottomY, duration).SetEase(curve).AsyncWaitForCompletion()
			.AsUniTask();

		await UniTask.WhenAll(upperMove, bottomMove);

		Hide();
	}

	public void SetMessage(string name)
	{
		locationName.text = name;
	}

	public async UniTask ShowHint()
	{
		await hintGroup
			.DOFade(1f, 0.5f)
			.SetEase(Ease.OutQuad)
			.AsyncWaitForCompletion();
	}

	public async UniTask HideHint()
	{
		await hintGroup
			.DOFade(0f, 0.5f)
			.SetEase(Ease.OutQuad)
			.AsyncWaitForCompletion();
	}

	public async UniTask ShowLocationName()
	{
		await stationNameCanvasGroup
			.DOFade(1f, 0.5f)
			.SetEase(Ease.OutQuad)
			.AsyncWaitForCompletion();
	}

	public async UniTask HideLocationName()
	{
		await stationNameCanvasGroup
			.DOFade(0f, 0.5f)
			.SetEase(Ease.OutQuad)
			.AsyncWaitForCompletion();
	}

	protected override void OnHide()
	{
		base.OnHide();
	}

	protected override void OnShow()
	{
		base.OnShow();
	}
}
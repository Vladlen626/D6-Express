using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Main.Scripts.UI;

namespace _Main.Scripts.Dice
{
	public enum DiceUpgradeVariantVisualState
	{
		Idle = 0,
		Highlighted = 1,
		Selected = 2
	}

	public class UIDiceUpgradeVariantView : MonoBehaviour
	{
		private const float IdleScale = 1f;
		private const float SelectedScale = 1.05f;
		private const float SettleDuration = 0.08f;
		private const float FloatingAmplitude = 4f;
		private const float FloatingDuration = 2.6f;

		[SerializeField]
		private RectTransform root;

		[SerializeField]
		private TextMeshProUGUI faceText;

		[SerializeField]
		private TextMeshProUGUI affectedStatText;

		[SerializeField]
		private TextMeshProUGUI deltaValueText;

		[SerializeField]
		private Image deltaBackground;

		[SerializeField]
		private Image selectedHighlightBackground;

		private ColorStyleRef positiveBackgroundColor;
		private ColorStyleRef negativeBackgroundColor;
		private ColorStyleRef neutralBackgroundColor;
		private ColorStyleRef selectedHighlightColor;
		private bool hasBackgroundStyleOverrides;
		private bool hasSelectedHighlightStyle;

		private Tween scaleTween;
		private Tween floatingTween;
		private Vector2 baseAnchoredPosition;
		private int face;

		public int Face => face;
		public bool IsValid => root && faceText && affectedStatText && deltaValueText;

		public void SetData(DiceUpgradeRouletteSlotData slotData)
		{
			face = slotData.Face;

			if (faceText)
			{
				faceText.text = slotData.Face.ToString();
			}

			if (affectedStatText)
			{
				affectedStatText.text = GetAffectedLabel(slotData);
			}

			if (deltaValueText)
			{
				deltaValueText.text = FormatDelta(slotData.DeltaValue);
			}

			ApplyDeltaBackgroundColor(slotData.VisualSign);
		}

		public void SetBackgroundColorStyles(
			ColorStyleRef positive,
			ColorStyleRef negative,
			ColorStyleRef neutral)
		{
			positiveBackgroundColor = positive;
			negativeBackgroundColor = negative;
			neutralBackgroundColor = neutral;
			hasBackgroundStyleOverrides = true;
		}

		public void SetSelectedHighlightStyle(ColorStyleRef selected)
		{
			selectedHighlightColor = selected;
			hasSelectedHighlightStyle = true;
		}

		public void SetVisualState(DiceUpgradeVariantVisualState state)
		{
			ApplySelectionHighlight(state == DiceUpgradeVariantVisualState.Selected);
			switch (state)
			{
				case DiceUpgradeVariantVisualState.Selected:
					AnimateScale(SelectedScale);
					break;
				default:
					AnimateScale(IdleScale);
					break;
			}
		}

		public void StartFloating(float phaseDelay)
		{
			if (!root)
			{
				return;
			}

			if (floatingTween != null && floatingTween.IsActive())
			{
				floatingTween.Kill();
			}

			baseAnchoredPosition = root.anchoredPosition;
			root.anchoredPosition = baseAnchoredPosition;
			floatingTween = root.DOAnchorPosY(baseAnchoredPosition.y + FloatingAmplitude, FloatingDuration)
				.SetEase(Ease.InOutSine)
				.SetLoops(-1, LoopType.Yoyo)
				.SetDelay(Mathf.Max(0f, phaseDelay));
		}

		private void OnDisable()
		{
			if (scaleTween != null && scaleTween.IsActive())
			{
				scaleTween.Kill();
			}

			scaleTween = null;
			if (floatingTween != null && floatingTween.IsActive())
			{
				floatingTween.Kill();
			}
			floatingTween = null;
			if (root)
			{
				root.anchoredPosition = baseAnchoredPosition;
				root.localScale = Vector3.one;
			}

			if (selectedHighlightBackground)
			{
				selectedHighlightBackground.gameObject.SetActive(false);
			}
		}

		private void ApplyDeltaBackgroundColor(int visualSign)
		{
			if (!deltaBackground)
			{
				throw new System.InvalidOperationException("Delta background image is not assigned.");
			}
			deltaBackground.color = ResolveSignedColor(visualSign);
		}

		private Color ResolveSignedColor(int visualSign)
		{
			if (!hasBackgroundStyleOverrides)
			{
				throw new System.InvalidOperationException("Background color styles are not assigned.");
			}

			return visualSign > 0 ? positiveBackgroundColor.Value : visualSign < 0 ? negativeBackgroundColor.Value : neutralBackgroundColor.Value;
		}

		private void ApplySelectionHighlight(bool isSelected)
		{
			if (!isSelected)
			{
				if (selectedHighlightBackground)
				{
					selectedHighlightBackground.gameObject.SetActive(false);
				}
				return;
			}

			if (!selectedHighlightBackground)
			{
				throw new System.InvalidOperationException("Selected highlight background is not assigned.");
			}

			if (!hasSelectedHighlightStyle)
			{
				throw new System.InvalidOperationException("Selected highlight color style is not assigned.");
			}

			selectedHighlightBackground.color = selectedHighlightColor.Value;
			selectedHighlightBackground.gameObject.SetActive(true);
		}

		private void AnimateScale(float targetScale)
		{
			if (!root)
			{
				return;
			}

			if (scaleTween != null && scaleTween.IsActive())
			{
				scaleTween.Kill();
			}

			var safeScale = Mathf.Max(0.01f, targetScale);
			scaleTween = root.DOScale(Vector3.one * safeScale, SettleDuration).SetEase(Ease.OutSine);
		}

		private static string GetAffectedLabel(DiceUpgradeRouletteSlotData slotData)
		{
			if (!string.IsNullOrWhiteSpace(slotData.AffectedLabel))
			{
				return slotData.AffectedLabel;
			}

			return slotData.AffectedStat switch
			{
				DiceUpgradeAffectedStat.Min => "Min",
				DiceUpgradeAffectedStat.Max => "Max",
				_ => "Bonus"
			};
		}

		private static string FormatDelta(int deltaValue)
		{
			if (deltaValue > 0)
			{
				return $"+{deltaValue}";
			}

			return deltaValue.ToString();
		}
	}
}

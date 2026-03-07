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
		private const float HighlightedScale = 1.02f;
		private const float SelectedScale = 1.05f;
		private const float HighlightWaveScaleMultiplier = 1.12f;
		private const float HighlightWaveDuration = 0.12f;
		private const float SettleDuration = 0.08f;

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

		private ColorStyleReference positiveBackgroundColor;
		private ColorStyleReference negativeBackgroundColor;
		private ColorStyleReference neutralBackgroundColor;
		private bool hasBackgroundStyleOverrides;

		private bool cachedBaseColors;
		private Color baseDeltaBackgroundColor;

		private Tween scaleTween;
		private int face;

		public int Face => face;
		public bool IsValid => root && faceText && affectedStatText && deltaValueText;

		private void Awake()
		{
			CacheBaseColors();
			ApplyDeltaBackgroundColor(0);
		}

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

			ApplyDeltaBackgroundColor(slotData.DeltaValue);
		}

		public void SetBackgroundColorStyles(
			ColorStyleReference positive,
			ColorStyleReference negative,
			ColorStyleReference neutral)
		{
			positiveBackgroundColor = positive;
			negativeBackgroundColor = negative;
			neutralBackgroundColor = neutral;
			hasBackgroundStyleOverrides = true;
		}

		public void SetVisualState(DiceUpgradeVariantVisualState state)
		{
			switch (state)
			{
				case DiceUpgradeVariantVisualState.Selected:
					AnimateScale(SelectedScale);
					break;
				case DiceUpgradeVariantVisualState.Highlighted:
					PlayHighlightWave();
					break;
				default:
					AnimateScale(IdleScale);
					break;
			}
		}

		private void OnDisable()
		{
			if (scaleTween != null && scaleTween.IsActive())
			{
				scaleTween.Kill();
			}

			scaleTween = null;
			if (root)
			{
				root.localScale = Vector3.one;
			}
		}

		private void ApplyDeltaBackgroundColor(int deltaValue)
		{
			if (!deltaBackground)
			{
				return;
			}
			deltaBackground.color = ResolveSignedColor(deltaValue, baseDeltaBackgroundColor);
		}

		private void CacheBaseColors()
		{
			if (cachedBaseColors)
			{
				return;
			}

			baseDeltaBackgroundColor = deltaBackground ? deltaBackground.color : Color.white;
			cachedBaseColors = true;
		}

		private Color ResolveSignedColor(int delta, Color fallback)
		{
			if (!hasBackgroundStyleOverrides)
			{
				return fallback;
			}

			var library = ColorStyleLibraryProvider.GetDefault();
			if (library == null)
			{
				return fallback;
			}

			var reference = delta > 0 ? positiveBackgroundColor : delta < 0 ? negativeBackgroundColor : neutralBackgroundColor;
			if (string.IsNullOrWhiteSpace(reference.Id))
			{
				return fallback;
			}

			var style = library.GetStyle(reference.Id);
			return style != null ? style.Color : fallback;
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

		private void PlayHighlightWave()
		{
			if (!root)
			{
				return;
			}

			if (scaleTween != null && scaleTween.IsActive())
			{
				scaleTween.Kill();
			}

			var baseScale = HighlightedScale;
			var peakScale = baseScale * HighlightWaveScaleMultiplier;
			root.localScale = Vector3.one * baseScale;

			scaleTween = DOTween.Sequence()
				.Append(root.DOScale(Vector3.one * peakScale, HighlightWaveDuration * 0.5f).SetEase(Ease.OutSine))
				.Append(root.DOScale(Vector3.one * baseScale, HighlightWaveDuration * 0.5f).SetEase(Ease.InSine));
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

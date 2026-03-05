using DG.Tweening;
using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;
using _Main.Scripts.UI;

namespace _Main.Scripts.Dice
{
	public class UIDiceUpgradeView : UIBaseElement
	{
		private const float DefaultValueAnimDuration = 2f;
		private static readonly Color DefaultChangedColor = new Color(1f, 0.82f, 0.2f);

		[SerializeField]
		private TextMeshProUGUI titleText;

		[SerializeField]
		private TextMeshProUGUI rolledFaceText;

		[SerializeField]
		private TextMeshProUGUI effectText;

		[SerializeField]
		private TextMeshProUGUI hintText;

		[SerializeField]
		private string normalValueStyleId;

		[SerializeField]
		private string changedValueStyleId;

		private Tween minTween;
		private Tween maxTween;
		private Tween bonusTween;
		private float minValue;
		private float maxValue;
		private float bonusValue;
		private bool minChanged;
		private bool maxChanged;
		private bool bonusChanged;
		private string minLabel;
		private string maxLabel;
		private string bonusLabel;
		private Color normalValueColor;
		private Color changedValueColor;

		public void SetData(DiceUpgradeVisualData data)
		{
			if (titleText)
			{
				titleText.text = string.IsNullOrWhiteSpace(data.Title) ? data.ComboId : data.Title;
			}

			if (rolledFaceText)
			{
				rolledFaceText.text = data.RolledText ?? string.Empty;
			}

			if (hintText)
			{
				hintText.text = data.HintText ?? string.Empty;
			}

			minLabel = data.MinLabel ?? string.Empty;
			maxLabel = data.MaxLabel ?? string.Empty;
			bonusLabel = data.BonusLabel ?? string.Empty;
			minChanged = data.BeforeMin != data.AfterMin;
			maxChanged = data.BeforeMax != data.AfterMax;
			bonusChanged = data.BeforeBonus != data.AfterBonus;

			ApplyStyle();
			StartValueAnimation(data);
		}

		private void OnDisable()
		{
			KillTweens();
		}

		private void ApplyStyle()
		{
			var baseColor = effectText ? effectText.color : Color.white;
			var library = TextStyleLibraryProvider.GetDefault();
			var normalStyle = library ? library.GetStyle(normalValueStyleId) : null;
			var changedStyle = library ? library.GetStyle(changedValueStyleId) : null;

			normalValueColor = normalStyle != null ? normalStyle.Color : baseColor;
			changedValueColor = changedStyle != null ? changedStyle.Color : DefaultChangedColor;
		}

		private void StartValueAnimation(DiceUpgradeVisualData data)
		{
			KillTweens();

			minValue = data.BeforeMin;
			maxValue = data.BeforeMax;
			bonusValue = data.BeforeBonus;
			RefreshEffectText();

			float duration = DefaultValueAnimDuration;

			minTween = DOTween.To(() => minValue, v =>
			{
				minValue = v;
				RefreshEffectText();
			}, data.AfterMin, duration).SetEase(Ease.OutQuad);

			maxTween = DOTween.To(() => maxValue, v =>
			{
				maxValue = v;
				RefreshEffectText();
			}, data.AfterMax, duration).SetEase(Ease.OutQuad);

			bonusTween = DOTween.To(() => bonusValue, v =>
			{
				bonusValue = v;
				RefreshEffectText();
			}, data.AfterBonus, duration).SetEase(Ease.OutQuad);
		}

		private void RefreshEffectText()
		{
			if (!effectText)
			{
				return;
			}

			var minPart = FormatLabelValue(minLabel, Mathf.RoundToInt(minValue), minChanged);
			var maxPart = FormatLabelValue(maxLabel, Mathf.RoundToInt(maxValue), maxChanged);
			var bonusPart = FormatLabelValue(bonusLabel, Mathf.RoundToInt(bonusValue), bonusChanged);

			effectText.text = $"{minPart} | {maxPart} | {bonusPart}";
		}

		private string FormatLabelValue(string label, int value, bool changed)
		{
			var color = changed ? changedValueColor : normalValueColor;
			var hex = ColorUtility.ToHtmlStringRGB(color);
			if (string.IsNullOrWhiteSpace(label))
			{
				return $"<color=#{hex}>{value}</color>";
			}

			return $"<color=#{hex}>{label} {value}</color>";
		}

		private void KillTweens()
		{
			if (minTween != null && minTween.IsActive())
			{
				minTween.Kill();
			}

			if (maxTween != null && maxTween.IsActive())
			{
				maxTween.Kill();
			}

			if (bonusTween != null && bonusTween.IsActive())
			{
				bonusTween.Kill();
			}
		}
	}
}

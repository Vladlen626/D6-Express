using System.Collections.Generic;
using DG.Tweening;
using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;
using _Main.Scripts.UI;

namespace _Main.Scripts.Dice
{
	public class UIDiceUpgradeView : UIBaseElement
	{
		private const float DefaultValueAnimDuration = 1.4f;
		private const float DefaultRouletteStepDuration = 0.09f;
		private const float DefaultRouletteRadius = 88f;
		private static readonly Color DefaultChangedColor = new Color(1f, 0.82f, 0.2f);

		[SerializeField] private TextMeshProUGUI titleText;

		[SerializeField] private TextMeshProUGUI effectText;

		[SerializeField] private TextMeshProUGUI hintText;

		[SerializeField] private RectTransform rouletteRoot;

		[SerializeField] private float rouletteRadius = DefaultRouletteRadius;

		[SerializeField] private string normalValueStyleId;

		[SerializeField] private string changedValueStyleId;

		[SerializeField] private float rouletteStepDuration = DefaultRouletteStepDuration;

		private readonly List<UIDiceUpgradeVariantView> spawnedVariants = new();
		private readonly List<DiceUpgradeRouletteSlotData> visibleSlots = new();

		private Tween minTween;
		private Tween maxTween;
		private Tween bonusTween;
		private Tween rouletteTween;
		private float minValue;
		private float maxValue;
		private float bonusValue;
		private int targetMinValue;
		private int targetMaxValue;
		private int targetBonusValue;
		private bool minChanged;
		private bool maxChanged;
		private bool bonusChanged;
		private bool showChangedValues;
		private string minLabel;
		private string maxLabel;
		private string bonusLabel;
		private string continueHintText;
		private string stopHintText;
		private Color normalValueColor;
		private Color changedValueColor;
		private int activeRouletteVariantIndex = -1;
		private int selectedRouletteVariantIndex = -1;
		private bool isRollResolved;
		private bool warnedMissingPrefab;
		private UIDiceUpgradeVariantView rouletteVariantPrefab;

		public void SetRouletteVariantPrefab(UIDiceUpgradeVariantView prefab)
		{
			rouletteVariantPrefab = prefab;
			if (rouletteVariantPrefab)
			{
				warnedMissingPrefab = false;
			}
		}

		public void SetData(DiceUpgradeVisualData data)
		{
			KillTweens();
			ApplyStyle();

			if (titleText)
			{
				titleText.text = string.IsNullOrWhiteSpace(data.Title) ? data.ComboId : data.Title;
			}

			continueHintText = data.HintText ?? string.Empty;
			stopHintText = string.IsNullOrWhiteSpace(data.StopHintText) ? continueHintText : data.StopHintText;

			if (hintText)
			{
				hintText.text = stopHintText;
			}

			minLabel = data.MinLabel ?? string.Empty;
			maxLabel = data.MaxLabel ?? string.Empty;
			bonusLabel = data.BonusLabel ?? string.Empty;
			minChanged = data.BeforeMin != data.AfterMin;
			maxChanged = data.BeforeMax != data.AfterMax;
			bonusChanged = data.BeforeBonus != data.AfterBonus;
			showChangedValues = false;
			isRollResolved = false;
			activeRouletteVariantIndex = -1;
			selectedRouletteVariantIndex = -1;

			minValue = data.BeforeMin;
			maxValue = data.BeforeMax;
			bonusValue = data.BeforeBonus;
			targetMinValue = data.AfterMin;
			targetMaxValue = data.AfterMax;
			targetBonusValue = data.AfterBonus;

			RebuildVisibleSlots(data.RouletteSlots);
			RebuildRouletteVariants();
			selectedRouletteVariantIndex = FindVariantIndexByFace(data.RolledFace);

			RefreshRouletteVisuals();
			RefreshEffectText();
			StartRouletteAnimation();
		}

		public void ResolveRoll()
		{
			if (isRollResolved)
			{
				return;
			}

			isRollResolved = true;
			showChangedValues = true;

			StopRouletteAnimation();
			RefreshRouletteVisuals();

			if (hintText)
			{
				hintText.text = continueHintText;
			}

			StartValueAnimation();
		}

		private void OnDisable()
		{
			KillTweens();
			ClearSpawnedVariants();
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

		private void StartValueAnimation()
		{
			KillValueTweens();

			float duration = DefaultValueAnimDuration;

			minTween = DOTween.To(() => minValue, v =>
			{
				minValue = v;
				RefreshEffectText();
			}, targetMinValue, duration).SetEase(Ease.OutQuad);

			maxTween = DOTween.To(() => maxValue, v =>
			{
				maxValue = v;
				RefreshEffectText();
			}, targetMaxValue, duration).SetEase(Ease.OutQuad);

			bonusTween = DOTween.To(() => bonusValue, v =>
			{
				bonusValue = v;
				RefreshEffectText();
			}, targetBonusValue, duration).SetEase(Ease.OutQuad);
		}

		private void StartRouletteAnimation()
		{
			StopRouletteAnimation();

			if (spawnedVariants.Count == 0)
			{
				return;
			}

			AdvanceRouletteHighlight();

			var step = Mathf.Max(0.02f, rouletteStepDuration);
			rouletteTween = DOTween.Sequence()
				.AppendInterval(step)
				.SetLoops(-1, LoopType.Restart)
				.OnStepComplete(AdvanceRouletteHighlight);
		}

		private void StopRouletteAnimation()
		{
			if (rouletteTween != null && rouletteTween.IsActive())
			{
				rouletteTween.Kill();
			}

			rouletteTween = null;
		}

		private void AdvanceRouletteHighlight()
		{
			if (spawnedVariants.Count == 0)
			{
				return;
			}

			activeRouletteVariantIndex = (activeRouletteVariantIndex + 1) % spawnedVariants.Count;
			RefreshRouletteVisuals();
		}

		private void RefreshRouletteVisuals()
		{
			for (int i = 0; i < spawnedVariants.Count; i++)
			{
				var variant = spawnedVariants[i];
				if (!variant || !variant.IsValid)
				{
					continue;
				}

				if (isRollResolved && i == selectedRouletteVariantIndex)
				{
					variant.SetVisualState(DiceUpgradeVariantVisualState.Selected);
					continue;
				}

				if (!isRollResolved && i == activeRouletteVariantIndex)
				{
					variant.SetVisualState(DiceUpgradeVariantVisualState.Highlighted);
					continue;
				}

				variant.SetVisualState(DiceUpgradeVariantVisualState.Idle);
			}
		}

		private void RebuildVisibleSlots(DiceUpgradeRouletteSlotData[] slotData)
		{
			visibleSlots.Clear();
			if (slotData == null || slotData.Length == 0)
			{
				return;
			}

			for (int i = 0; i < slotData.Length; i++)
			{
				var slot = slotData[i];
				if (IsZeroBonus(slot.BonusText))
				{
					continue;
				}

				visibleSlots.Add(slot);
			}
		}

		private void RebuildRouletteVariants()
		{
			ClearSpawnedVariants();
			var root = ResolveRouletteRoot();
			DisableLegacyRouletteVariants(root);
			if (visibleSlots.Count == 0)
			{
				return;
			}

			if (!root)
			{
				Debug.LogWarning("[UIDiceUpgradeView] Roulette root is not assigned.");
				return;
			}

			var variantPrefab = ResolveRouletteVariantPrefab();
			if (!variantPrefab)
			{
				if (!warnedMissingPrefab)
				{
					Debug.LogWarning(
						"[UIDiceUpgradeView] Roulette variant prefab is not provided.");
					warnedMissingPrefab = true;
				}

				return;
			}

			var count = visibleSlots.Count;
			var safeRadius = Mathf.Max(0f, rouletteRadius);
			for (int i = 0; i < count; i++)
			{
				var instance = Instantiate(variantPrefab, root);
				instance.gameObject.SetActive(true);
				instance.SetData(visibleSlots[i].Face, visibleSlots[i].BonusText);

				var rect = instance.GetComponent<RectTransform>();
				if (rect)
				{
					rect.anchorMin = new Vector2(0.5f, 0.5f);
					rect.anchorMax = new Vector2(0.5f, 0.5f);
					rect.pivot = new Vector2(0.5f, 0.5f);
					rect.anchoredPosition = GetCirclePosition(i, count, safeRadius);
					rect.localScale = Vector3.one;
				}

				spawnedVariants.Add(instance);
			}
		}

		private RectTransform ResolveRouletteRoot()
		{
			if (rouletteRoot)
			{
				return rouletteRoot;
			}

			var fallbackRoot = transform.Find("RouletteRoot");
			if (fallbackRoot)
			{
				rouletteRoot = fallbackRoot as RectTransform;
			}

			return rouletteRoot;
		}

		private UIDiceUpgradeVariantView ResolveRouletteVariantPrefab()
		{
			if (rouletteVariantPrefab)
			{
				warnedMissingPrefab = false;
				return rouletteVariantPrefab;
			}

			return null;
		}

		private void DisableLegacyRouletteVariants(RectTransform root)
		{
			if (!root)
			{
				return;
			}

			for (int i = 0; i < root.childCount; i++)
			{
				var child = root.GetChild(i);
				if (!child)
				{
					continue;
				}

				var variant = child.GetComponent<UIDiceUpgradeVariantView>();
				if (variant)
				{
					variant.gameObject.SetActive(false);
				}
			}
		}

		private void ClearSpawnedVariants()
		{
			for (int i = 0; i < spawnedVariants.Count; i++)
			{
				var variant = spawnedVariants[i];
				if (variant)
				{
					Destroy(variant.gameObject);
				}
			}

			spawnedVariants.Clear();
		}

		private int FindVariantIndexByFace(int face)
		{
			for (int i = 0; i < visibleSlots.Count; i++)
			{
				if (visibleSlots[i].Face == face)
				{
					return i;
				}
			}

			return -1;
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
			var color = showChangedValues && changed ? changedValueColor : normalValueColor;
			var hex = ColorUtility.ToHtmlStringRGB(color);
			if (string.IsNullOrWhiteSpace(label))
			{
				return $"<color=#{hex}>{value}</color>";
			}

			return $"<color=#{hex}>{label} {value}</color>";
		}

		private void KillTweens()
		{
			KillValueTweens();
			StopRouletteAnimation();
		}

		private void KillValueTweens()
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

		private static Vector2 GetCirclePosition(int index, int count, float radius)
		{
			if (count <= 0)
			{
				return Vector2.zero;
			}

			var angleStep = 360f / count;
			var startAngle = 90f - (angleStep * 0.5f);
			var angle = (startAngle - index * angleStep) * Mathf.Deg2Rad;
			return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
		}

		private static bool IsZeroBonus(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return true;
			}

			var normalized = text.Trim();
			if (normalized.StartsWith("+"))
			{
				normalized = normalized.Substring(1);
			}

			return int.TryParse(normalized, out var value) && value == 0;
		}
	}
}

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

		[SerializeField]
		private TextMeshProUGUI titleText;

		[SerializeField]
		private TextMeshProUGUI hintText;

		[SerializeField]
		private TextMeshProUGUI minValueText;

		[SerializeField]
		private TextMeshProUGUI maxValueText;

		[SerializeField]
		private TextMeshProUGUI bonusValueText;

		[SerializeField]
		private RectTransform rouletteRoot;

		[SerializeField]
		private float rouletteRadius = DefaultRouletteRadius;

		[SerializeField]
		private float rouletteStepDuration = DefaultRouletteStepDuration;

		[SerializeField]
		private ColorStyleReference positiveChangeColor;

		[SerializeField]
		private ColorStyleReference negativeChangeColor;

		[SerializeField]
		private ColorStyleReference neutralChangeColor;

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
		private string minLabel;
		private string maxLabel;
		private string bonusLabel;
		private string continueHintText;
		private string stopHintText;
		private int activeRouletteVariantIndex = -1;
		private int selectedRouletteVariantIndex = -1;
		private bool isRollResolved;
		private bool warnedMissingPrefab;
		private UIDiceUpgradeVariantView rouletteVariantPrefab;
		private bool cachedBaseColors;
		private Color minBaseColor;
		private Color maxBaseColor;
		private Color bonusBaseColor;
		private DiceUpgradeAffectedStat selectedAffectedStat = DiceUpgradeAffectedStat.None;
		private int selectedDeltaValue;

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
			CacheBaseColors();

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
			isRollResolved = false;
			selectedAffectedStat = DiceUpgradeAffectedStat.None;
			selectedDeltaValue = 0;
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
			RefreshStatTexts();
			StartRouletteAnimation();
		}

		public void ApplyRollResult()
		{
			if (isRollResolved)
			{
				return;
			}

			isRollResolved = true;
			UpdateResolvedSlotState();
			StopRouletteAnimation();
			RefreshRouletteVisuals();
			RefreshStatTexts();

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

		private void StartValueAnimation()
		{
			KillValueTweens();

			float duration = DefaultValueAnimDuration;

			if (minChanged)
			{
				minTween = DOTween.To(() => minValue, v =>
				{
					minValue = v;
					RefreshStatTexts();
				}, targetMinValue, duration).SetEase(Ease.OutQuad);
			}
			else
			{
				minValue = targetMinValue;
			}

			if (maxChanged)
			{
				maxTween = DOTween.To(() => maxValue, v =>
				{
					maxValue = v;
					RefreshStatTexts();
				}, targetMaxValue, duration).SetEase(Ease.OutQuad);
			}
			else
			{
				maxValue = targetMaxValue;
			}

			if (bonusChanged)
			{
				bonusTween = DOTween.To(() => bonusValue, v =>
				{
					bonusValue = v;
					RefreshStatTexts();
				}, targetBonusValue, duration).SetEase(Ease.OutQuad);
			}
			else
			{
				bonusValue = targetBonusValue;
			}

			RefreshStatTexts();
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
				for (int face = 1; face <= 6; face++)
				{
					visibleSlots.Add(new DiceUpgradeRouletteSlotData(
						face,
						DiceUpgradeAffectedStat.Bonus,
						0,
						bonusLabel));
				}

				return;
			}

			for (int i = 0; i < slotData.Length; i++)
			{
				visibleSlots.Add(slotData[i]);
			}
		}

		private void RebuildRouletteVariants()
		{
			ClearSpawnedVariants();
			if (visibleSlots.Count == 0)
			{
				return;
			}

			if (!rouletteRoot)
			{
				Debug.LogWarning("[UIDiceUpgradeView] Roulette root is not assigned.");
				return;
			}

			if (!rouletteVariantPrefab)
			{
				if (!warnedMissingPrefab)
				{
					Debug.LogWarning("[UIDiceUpgradeView] Roulette variant prefab is not provided.");
					warnedMissingPrefab = true;
				}

				return;
			}

			var count = visibleSlots.Count;
			var safeRadius = Mathf.Max(0f, rouletteRadius);
			for (int i = 0; i < count; i++)
			{
				var instance = Instantiate(rouletteVariantPrefab, rouletteRoot);
				instance.gameObject.SetActive(true);
				instance.SetBackgroundColorStyles(positiveChangeColor, negativeChangeColor, neutralChangeColor);
				instance.SetData(visibleSlots[i]);

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

		private void UpdateResolvedSlotState()
		{
			if (selectedRouletteVariantIndex < 0 || selectedRouletteVariantIndex >= visibleSlots.Count)
			{
				selectedAffectedStat = DiceUpgradeAffectedStat.None;
				selectedDeltaValue = 0;
				return;
			}

			var slot = visibleSlots[selectedRouletteVariantIndex];
			selectedAffectedStat = slot.AffectedStat;
			selectedDeltaValue = slot.DeltaValue;
		}

		private void RefreshStatTexts()
		{
			RefreshStatText(minValueText, minLabel, Mathf.RoundToInt(minValue), DiceUpgradeAffectedStat.Min, minBaseColor);
			RefreshStatText(maxValueText, maxLabel, Mathf.RoundToInt(maxValue), DiceUpgradeAffectedStat.Max, maxBaseColor);
			RefreshStatText(bonusValueText, bonusLabel, Mathf.RoundToInt(bonusValue), DiceUpgradeAffectedStat.Bonus, bonusBaseColor);
		}

		private void RefreshStatText(
			TextMeshProUGUI targetText,
			string label,
			int value,
			DiceUpgradeAffectedStat stat,
			Color baseColor)
		{
			if (!targetText)
			{
				return;
			}

			targetText.color = GetStatTextColor(stat, baseColor);
			if (string.IsNullOrWhiteSpace(label))
			{
				targetText.text = value.ToString();
				return;
			}

			targetText.text = $"{label} {value}";
		}

		private Color GetStatTextColor(DiceUpgradeAffectedStat stat, Color baseColor)
		{
			if (!isRollResolved || selectedAffectedStat != stat)
			{
				return baseColor;
			}

			return ResolveSignedColor(selectedDeltaValue, baseColor);
		}

		private Color ResolveSignedColor(int delta, Color fallback)
		{
			var library = ColorStyleLibraryProvider.GetDefault();
			if (library == null)
			{
				return fallback;
			}

			var reference = delta > 0 ? positiveChangeColor : delta < 0 ? negativeChangeColor : neutralChangeColor;
			if (string.IsNullOrWhiteSpace(reference.Id))
			{
				return fallback;
			}

			var style = library.GetStyle(reference.Id);
			return style != null ? style.Color : fallback;
		}

		private void CacheBaseColors()
		{
			if (cachedBaseColors)
			{
				return;
			}

			minBaseColor = minValueText ? minValueText.color : Color.white;
			maxBaseColor = maxValueText ? maxValueText.color : Color.white;
			bonusBaseColor = bonusValueText ? bonusValueText.color : Color.white;
			cachedBaseColors = true;
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

			minTween = null;
			maxTween = null;
			bonusTween = null;
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
	}
}

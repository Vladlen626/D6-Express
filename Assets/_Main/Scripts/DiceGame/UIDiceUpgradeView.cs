using System;
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
		private const float DefaultRouletteRadius = 88f;
		private const float ResolvedTopPositionOffset = 0f;

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
		private ColorStyleRef positiveChangeColor;

		[SerializeField]
		private ColorStyleRef negativeChangeColor;

		[SerializeField]
		private ColorStyleRef neutralChangeColor;

		[SerializeField]
		private ColorStyleRef selectedVariantHighlightColor;

		private readonly List<UIDiceUpgradeVariantView> spawnedVariants = new();
		private readonly List<DiceUpgradeRouletteSlotData> visibleSlots = new();

		private Tween minTween;
		private Tween maxTween;
		private Tween bonusTween;
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
		private int selectedRouletteVariantIndex = -1;
		private bool isRollResolved;
		private bool hasResolvedRouletteAnimation;
		private UIDiceUpgradeVariantView rouletteVariantPrefab;
		private bool cachedBaseColors;
		private Color minBaseColor;
		private Color maxBaseColor;
		private Color bonusBaseColor;
		private DiceUpgradeAffectedStat selectedAffectedStat = DiceUpgradeAffectedStat.None;
		private int selectedVisualSign;
		private int rolledFace;

		public void SetRouletteVariantPrefab(UIDiceUpgradeVariantView prefab)
		{
			rouletteVariantPrefab = prefab;
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
			hasResolvedRouletteAnimation = false;
			selectedAffectedStat = DiceUpgradeAffectedStat.None;
			selectedVisualSign = 0;
			selectedRouletteVariantIndex = -1;
			rolledFace = data.RolledFace;

			minValue = data.BeforeMin;
			maxValue = data.BeforeMax;
			bonusValue = data.BeforeBonus;
			targetMinValue = data.AfterMin;
			targetMaxValue = data.AfterMax;
			targetBonusValue = data.AfterBonus;

			RebuildVisibleSlots(data.RouletteSlots);
			RebuildRouletteVariants();
			selectedRouletteVariantIndex = GetVariantIndexFromFace(data.RolledFace, visibleSlots.Count);
			LogFaceDebug("SetData");

			RefreshRouletteVisuals();
			RefreshStatTexts();
		}

		public void ApplyRollResult()
		{
			if (isRollResolved)
			{
				return;
			}

			isRollResolved = true;
			UpdateResolvedSlotState();
			LogFaceDebug("ApplyRollResult");
			PlayResolvedRouletteAnimation();
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
				}
				else if (!isRollResolved)
				{
					variant.SetVisualState(DiceUpgradeVariantVisualState.Idle);
				}
			}
		}

		private void RebuildVisibleSlots(DiceUpgradeRouletteSlotData[] slotData)
		{
			visibleSlots.Clear();
			var orderedSlots = new DiceUpgradeRouletteSlotData[6];
			for (int i = 0; i < orderedSlots.Length; i++)
			{
				var face = i + 1;
				orderedSlots[i] = new DiceUpgradeRouletteSlotData(face, DiceUpgradeAffectedStat.Bonus, 0, bonusLabel);
			}

			if (slotData == null || slotData.Length == 0)
			{
				visibleSlots.AddRange(orderedSlots);
				return;
			}

			for (int i = 0; i < slotData.Length; i++)
			{
				var slot = slotData[i];
				var index = GetVariantIndexFromFace(slot.Face, orderedSlots.Length);
				if (index < 0)
				{
					continue;
				}

				orderedSlots[index] = slot;
			}

			visibleSlots.AddRange(orderedSlots);
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
				throw new InvalidOperationException("Roulette root is not assigned.");
			}

			if (!rouletteVariantPrefab)
			{
				throw new InvalidOperationException("Roulette variant prefab is not assigned.");
			}

			if (string.IsNullOrWhiteSpace(selectedVariantHighlightColor.Id))
			{
				throw new InvalidOperationException("Selected variant highlight color style is not assigned.");
			}

			var count = visibleSlots.Count;
			var safeRadius = Mathf.Max(0f, rouletteRadius);
			for (int i = 0; i < count; i++)
			{
				var instance = Instantiate(rouletteVariantPrefab, rouletteRoot);
				instance.gameObject.SetActive(true);
				instance.SetBackgroundColorStyles(positiveChangeColor, negativeChangeColor, neutralChangeColor);
				instance.SetSelectedHighlightStyle(selectedVariantHighlightColor);
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

				instance.StartFloating(i * 0.18f);
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

		private void UpdateResolvedSlotState()
		{
			if (selectedRouletteVariantIndex < 0 || selectedRouletteVariantIndex >= visibleSlots.Count)
			{
				selectedAffectedStat = DiceUpgradeAffectedStat.None;
				selectedVisualSign = 0;
				return;
			}

			var slot = visibleSlots[selectedRouletteVariantIndex];
			selectedAffectedStat = slot.AffectedStat;
			selectedVisualSign = slot.VisualSign;
		}

		private void PlayResolvedRouletteAnimation()
		{
			if (hasResolvedRouletteAnimation)
			{
				return;
			}

			if (selectedRouletteVariantIndex < 0 || selectedRouletteVariantIndex >= spawnedVariants.Count)
			{
				return;
			}

			hasResolvedRouletteAnimation = true;
			var topPosition = new Vector2(0f, Mathf.Max(0f, rouletteRadius) + ResolvedTopPositionOffset);
			for (int i = 0; i < spawnedVariants.Count; i++)
			{
				var variant = spawnedVariants[i];
				if (!variant || !variant.IsValid)
				{
					continue;
				}

				if (i == selectedRouletteVariantIndex)
				{
					variant.AnimateResolveSelected(topPosition);
					continue;
				}

				variant.AnimateResolveHidden();
			}
		}

		private void LogFaceDebug(string stage)
		{
			if (selectedRouletteVariantIndex < 0 || selectedRouletteVariantIndex >= visibleSlots.Count)
			{
				Debug.LogWarning($"[UpgradeDebug:{stage}] rolledFace={rolledFace}, selectedCircleFace=invalid, selectedIndex={selectedRouletteVariantIndex}, slotsCount={visibleSlots.Count}");
				return;
			}

			var slot = visibleSlots[selectedRouletteVariantIndex];
			Debug.Log($"[UpgradeDebug:{stage}] rolledFace={rolledFace}, selectedCircleFace={slot.Face}, selectedIndex={selectedRouletteVariantIndex}, stat={slot.AffectedStat}, delta={slot.DeltaValue}");
		}

		private static int GetVariantIndexFromFace(int face, int count)
		{
			var index = face - 1;
			return index >= 0 && index < count ? index : -1;
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

			return ResolveSignedColor(selectedVisualSign);
		}

		private Color ResolveSignedColor(int visualSign)
		{
			return visualSign > 0 ? positiveChangeColor.Value : visualSign < 0 ? negativeChangeColor.Value : neutralChangeColor.Value;
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

using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
		private const string StateBackgroundName = "StateBackground";
		private const string ValueTextName = "ValueText";
		private const string ValueBonusSeparator = "------";
		private const float IdleScale = 1f;
		private const float HighlightedScale = 1.02f;
		private const float SelectedScale = 1.04f;
		private const float HighlightWaveScaleMultiplier = 1.14f;
		private const float HighlightWaveDuration = 0.12f;
		private const float SettleDuration = 0.08f;
		private const float StateBackgroundScale = 1.06f;

		[SerializeField]
		private RectTransform root;

		[SerializeField]
		private TextMeshProUGUI valueText;

		[SerializeField]
		private Image stateBackground;

		[SerializeField]
		private Color highlightedBackgroundColor = new Color(1f, 0.92f, 0.45f, 1f);

		[SerializeField]
		private Color selectedBackgroundColor = new Color(0.66f, 1f, 0.66f, 1f);

		private Tween scaleTween;
		private int face;

		public int Face => face;
		public bool IsValid => root && valueText;

		private void Awake()
		{
			EnsureReferences();
			HideStateBackground();
		}

		public void SetData(int faceValue, string bonusText)
		{
			EnsureReferences();
			face = faceValue;
			if (!valueText)
			{
				return;
			}

			var bonus = string.IsNullOrWhiteSpace(bonusText) ? "0" : bonusText;
			valueText.text = $"[{faceValue}]\n{ValueBonusSeparator}\n{bonus}";
		}

		public void SetVisualState(DiceUpgradeVariantVisualState state)
		{
			EnsureReferences();
			switch (state)
			{
				case DiceUpgradeVariantVisualState.Selected:
					ShowStateBackground(selectedBackgroundColor);
					AnimateScale(SelectedScale);
					break;
				case DiceUpgradeVariantVisualState.Highlighted:
					ShowStateBackground(highlightedBackgroundColor);
					PlayHighlightWave();
					break;
				default:
					HideStateBackground();
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
			HideStateBackground();
		}

		private void EnsureReferences()
		{
			if (!root)
			{
				root = transform as RectTransform;
			}

			if (!root)
			{
				return;
			}

			if (!valueText)
			{
				valueText = root.GetComponentInChildren<TextMeshProUGUI>(true);
			}

			if (!valueText)
			{
				valueText = CreateValueText(root);
			}

			if (!stateBackground)
			{
				stateBackground = FindStateBackground(root);
			}

			if (!stateBackground)
			{
				stateBackground = CreateStateBackground(root);
			}
		}

		private void ShowStateBackground(Color color)
		{
			var background = ResolveStateBackground();
			if (!background)
			{
				return;
			}

			background.gameObject.SetActive(true);
			background.color = color;

			var backgroundRect = background.rectTransform;
			if (backgroundRect)
			{
				backgroundRect.SetAsFirstSibling();
				backgroundRect.localScale = Vector3.one * StateBackgroundScale;
			}
		}

		private void HideStateBackground()
		{
			var background = ResolveStateBackground();
			if (!background)
			{
				return;
			}

			background.gameObject.SetActive(false);
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

		private Image ResolveStateBackground()
		{
			EnsureReferences();
			if (stateBackground)
			{
				return stateBackground;
			}

			if (!root)
			{
				return null;
			}

			return FindStateBackground(root);
		}

		private static Image FindStateBackground(RectTransform parent)
		{
			Image firstImage = null;
			for (int i = 0; i < parent.childCount; i++)
			{
				var child = parent.GetChild(i);
				if (!child)
				{
					continue;
				}

				var image = child.GetComponent<Image>();
				if (!image)
				{
					continue;
				}

				if (child.name == StateBackgroundName)
				{
					return image;
				}

				if (!firstImage)
				{
					firstImage = image;
				}
			}

			return firstImage;
		}

		private static Image CreateStateBackground(RectTransform parent)
		{
			var gameObject = new GameObject(StateBackgroundName, typeof(RectTransform), typeof(Image));
			var rectTransform = gameObject.GetComponent<RectTransform>();
			rectTransform.SetParent(parent, false);
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			rectTransform.localScale = Vector3.one;
			rectTransform.localRotation = Quaternion.identity;
			rectTransform.anchoredPosition = Vector2.zero;

			var image = gameObject.GetComponent<Image>();
			image.raycastTarget = false;
			gameObject.SetActive(false);
			return image;
		}

		private static TextMeshProUGUI CreateValueText(RectTransform parent)
		{
			var gameObject = new GameObject(ValueTextName, typeof(RectTransform), typeof(TextMeshProUGUI));
			var rectTransform = gameObject.GetComponent<RectTransform>();
			rectTransform.SetParent(parent, false);
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			rectTransform.localScale = Vector3.one;
			rectTransform.localRotation = Quaternion.identity;
			rectTransform.anchoredPosition = Vector2.zero;

			var text = gameObject.GetComponent<TextMeshProUGUI>();
			text.alignment = TextAlignmentOptions.Center;
			text.raycastTarget = false;
			text.enableWordWrapping = false;
			text.fontSize = 18;
			return text;
		}
	}
}

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Main.Scripts.Dice
{
	public class DiceCombinationCardView : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI combinationNameText;

		[SerializeField]
		private TextMeshProUGUI scoreText;

		[SerializeField]
		private RectTransform diceFacesRoot;

		[SerializeField]
		private Image diceFaceIconPrefab;

		[SerializeField]
		private Sprite[] diceFaceSprites;

		[SerializeField]
		private RectTransform flyOrigin;

		[SerializeField]
		private float scoreAnimationDuration = 0.24f;

		[SerializeField]
		private float visibilityAnimationDuration = 0.15f;

		private readonly List<Image> faceIcons = new();
		private readonly List<int> cachedFaces = new(6);
		private Tween scoreTween;
		private Tween visibilityTween;
		private int currentScore;
		private bool isScoreInitialized;
		private bool isShown;

		public RectTransform FlyOrigin => flyOrigin;
		public bool IsShown => isShown;

		private void Awake()
		{
			ValidateReferences();
			isShown = transform.localScale.x > 0.001f;
		}

		private void OnDestroy()
		{
			if (scoreTween != null && scoreTween.IsActive())
			{
				scoreTween.Kill();
			}

			if (visibilityTween != null && visibilityTween.IsActive())
			{
				visibilityTween.Kill();
			}

			scoreTween = null;
			visibilityTween = null;
		}

		public void SetPresentation(string combinationName, IReadOnlyList<int> faces)
		{
			var safeCombinationName = combinationName ?? string.Empty;
			if (!string.Equals(combinationNameText.text, safeCombinationName, StringComparison.Ordinal))
			{
				combinationNameText.text = safeCombinationName;
			}

			ApplyFaces(faces);
		}

		public void SetScoreImmediate(int value)
		{
			if (isScoreInitialized && currentScore == value)
			{
				return;
			}

			currentScore = value;
			isScoreInitialized = true;
			scoreText.SetText("{0:0}", currentScore);
		}

		public void PrewarmFaceIcons(int iconsCount)
		{
			EnsureFaceIconCount(Mathf.Max(0, iconsCount));
		}

		public void SetVisibleImmediate(bool visible)
		{
			if (visibilityTween != null && visibilityTween.IsActive())
			{
				visibilityTween.Kill();
				visibilityTween = null;
			}

			transform.localScale = visible ? Vector3.one : Vector3.zero;
			isShown = visible;
		}

		public async UniTask AnimateShowAsync()
		{
			if (isShown)
			{
				return;
			}

			if (visibilityTween != null && visibilityTween.IsActive())
			{
				visibilityTween.Kill();
			}

			transform.localScale = Vector3.zero;
			visibilityTween = transform.DOScale(Vector3.one, visibilityAnimationDuration)
				.SetEase(Ease.OutQuad);
			await visibilityTween.AsyncWaitForCompletion().AsUniTask();
			visibilityTween = null;
			isShown = true;
		}

		public async UniTask AnimateHideAsync()
		{
			if (!isShown)
			{
				return;
			}

			if (visibilityTween != null && visibilityTween.IsActive())
			{
				visibilityTween.Kill();
			}

			visibilityTween = transform.DOScale(Vector3.zero, visibilityAnimationDuration)
				.SetEase(Ease.InQuad);
			await visibilityTween.AsyncWaitForCompletion().AsUniTask();
			visibilityTween = null;
			isShown = false;
		}

		private void ApplyFaces(IReadOnlyList<int> faces)
		{
			if (IsFacesPresentationUnchanged(faces))
			{
				return;
			}

			var facesCount = faces?.Count ?? 0;
			EnsureFaceIconCount(facesCount);

			cachedFaces.Clear();
			for (int i = 0; i < faceIcons.Count; i++)
			{
				var icon = faceIcons[i];
				var isVisible = i < facesCount;
				if (icon.gameObject.activeSelf != isVisible)
				{
					icon.gameObject.SetActive(isVisible);
				}

				if (!isVisible)
				{
					continue;
				}

				var face = faces[i];
				if (face < 1 || face > 6)
				{
					throw new InvalidOperationException(
						$"[DiceCombinationCardView] Face '{face}' is out of range [1..6].");
				}

				var sprite = diceFaceSprites[face - 1];
				if (icon.sprite != sprite)
				{
					icon.sprite = sprite;
				}

				cachedFaces.Add(face);
			}
		}

		private bool IsFacesPresentationUnchanged(IReadOnlyList<int> faces)
		{
			var facesCount = faces?.Count ?? 0;
			if (cachedFaces.Count != facesCount)
			{
				return false;
			}

			for (int i = 0; i < facesCount; i++)
			{
				if (cachedFaces[i] != faces[i])
				{
					return false;
				}
			}

			return true;
		}

		private void EnsureFaceIconCount(int requiredCount)
		{
			while (faceIcons.Count < requiredCount)
			{
				var icon = Instantiate(diceFaceIconPrefab, diceFacesRoot);
				faceIcons.Add(icon);
			}
		}

		private void ValidateReferences()
		{
			if (!combinationNameText || !scoreText || !diceFacesRoot || !diceFaceIconPrefab || !flyOrigin)
			{
				throw new MissingReferenceException(
					"[DiceCombinationCardView] Assign combinationNameText, scoreText, diceFacesRoot, diceFaceIconPrefab and flyOrigin.");
			}

			if (diceFaceSprites == null || diceFaceSprites.Length != 6)
			{
				throw new InvalidOperationException("[DiceCombinationCardView] Dice face sprites array must contain exactly 6 entries.");
			}

			for (int i = 0; i < diceFaceSprites.Length; i++)
			{
				if (!diceFaceSprites[i])
				{
					throw new MissingReferenceException($"[DiceCombinationCardView] Dice face sprite at index {i} is not assigned.");
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using DG.Tweening;
using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Main.Scripts.UI
{
	public class UITooltip : UIBaseElement
	{
		[SerializeField]
		private Transform tooltipTransform;

		[SerializeField] 
		private TextMeshProUGUI header;

		[SerializeField] 
		private TextMeshProUGUI description;

		[SerializeField]
		private GameObject activationLabelRoot;

		[SerializeField]
		private TextMeshProUGUI activationLabelText;

		[SerializeField]
		private Image activationLabelBackground;

		[SerializeField] 
		private ColorStyleRef inMatchColor;

		[SerializeField] 
		private ColorStyleRef preMatchColor;

		[SerializeField] 
		private RectTransform tooltipRectTransform;

		[SerializeField]
		private Vector2 staticAnchoredPosition;
		
		[SerializeField]
		private List<TooltipSeparatorEntry> tooltipEntries;

		private bool activationLabelSetupErrorLogged;
 
		public void ShowTooltip()
		{
			tooltipTransform.DOScale(Vector3.one, 0.075f);
		}

		public void HideTooltip()
		{
			tooltipTransform.DOScale(Vector3.zero, 0.035f);
		}

		public void SetHeaderText(string text)
		{
			header.text = text;
		}

		public void SetDescriptionText(string text)
		{
			description.text = text;
		}

		public void SetActivationLabel(string text, TooltipActivationLabelStyle? style = null)
		{
			if (!TryValidateActivationLabelSetup())
			{
				return;
			}

			var hasLabel = !string.IsNullOrWhiteSpace(text);
			activationLabelRoot.SetActive(hasLabel);
			if (!hasLabel)
			{
				return;
			}

			if (!style.HasValue)
			{
				throw new InvalidOperationException(
					"[UITooltip] Activation label style is not specified for a non-empty activation label.");
			}

			activationLabelText.text = text;
			activationLabelBackground.color = style.Value switch
			{
				TooltipActivationLabelStyle.InMatch => inMatchColor.Value,
				_ => preMatchColor.Value
			};
		}

		public void SetRarity(Rarity rarity)
		{
			foreach (var tooltipSeparatorEntry in tooltipEntries)
			{
				tooltipSeparatorEntry.GameObject.SetActive(tooltipSeparatorEntry.Rarity == rarity);
			}
		}

		public void SetPositionFromWorld(
			Transform worldTarget,
			Vector3 worldOffset,
			Camera mainCamera
		)
		{
			if (!worldTarget)
			{
				return;
			}

			Vector3 worldPos = worldTarget.position + worldOffset;
			Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

			if (screenPos.z < 0)
			{
				return;
			}

			tooltipRectTransform.position = screenPos;
		}

		public void SetStaticPosition()
		{
			tooltipRectTransform.anchoredPosition = staticAnchoredPosition;
		}

		private bool TryValidateActivationLabelSetup()
		{
			if (activationLabelRoot && activationLabelText && activationLabelBackground)
			{
				return true;
			}

			if (!activationLabelSetupErrorLogged)
			{
				activationLabelSetupErrorLogged = true;
				Debug.LogError("[UITooltip] Activation label references are not assigned.", this);
			}

			return false;
		}
	}

	public enum TooltipActivationLabelStyle
	{
		PreMatch,
		InMatch
	}

	[Serializable]
	public class TooltipSeparatorEntry
	{
		public Rarity Rarity;
		public GameObject GameObject;
	}
}

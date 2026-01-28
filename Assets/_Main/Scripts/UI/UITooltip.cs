using System;
using System.Collections.Generic;
using DG.Tweening;
using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

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
		private RectTransform tooltipRectTransform;
		
		[SerializeField]
		private List<TooltipSeparatorEntry> tooltipEntries;
 
		public void ShowTooltip()
		{
			tooltipTransform.DOScale(Vector3.one, 0.02f);
		}

		public void HideTooltip()
		{
			tooltipTransform.DOScale(Vector3.zero, 0.01f);
		}

		public void SetHeaderText(string text)
		{
			header.text = text;
		}

		public void SetDescriptionText(string text)
		{
			description.text = text;
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
	}

	[Serializable]
	public class TooltipSeparatorEntry
	{
		public Rarity Rarity;
		public GameObject GameObject;
	}
}
using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

namespace _Main.Scripts.UI
{
	public class UITooltip : UIBaseElement
	{
		[SerializeField] private TextMeshProUGUI header;

		[SerializeField] private TextMeshProUGUI description;

		[SerializeField] private RectTransform tooltipRectTransform;

		public void SetHeaderText(string text)
		{
			header.text = text;
		}

		public void SetDescriptionText(string text)
		{
			description.text = text;
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
}
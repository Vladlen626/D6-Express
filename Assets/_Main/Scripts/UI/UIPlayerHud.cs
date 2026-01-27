using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

namespace _Main.Scripts.UI
{
	public class UIPlayerHud : UIBaseElement
	{
		[SerializeField]
		private TextMeshProUGUI cashCountText;

		[SerializeField]
		private LocalizedText ticks;

		[SerializeField]
		private LocalizedText days;

		[SerializeField]
		private LocalizedText cashProgress;

		public void SetCashCountText(string text)
		{
			cashCountText.text = text;
		}

		public void SetTicksText(string id, params string[] args)
		{
			ticks.SetText(id, args);
		}

		public void SetDaysText(string id, params string[] args)
		{
			days.SetText(id, args);
		}

		public void SetCashProgressText(string id, params string[] args)
		{
			cashProgress.SetText(id, args);
		}
	}
}
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

		public void SetCashCountText(string text)
		{
			cashCountText.text = text;
		}

		public void SetCashCountText(int value)
		{
			cashCountText.SetText("$: {0:0}", value);
		}

		public void SetTicksText(string id, params string[] args)
		{
			ticks.SetText(id, args);
		}

		public void SetTicksText(string id, int value)
		{
			ticks.SetText(id, value);
		}

		public void SetDaysText(string id, params string[] args)
		{
			days.SetText(id, args);
		}

		public void SetDaysText(string id, int value)
		{
			days.SetText(id, value);
		}
	}
}
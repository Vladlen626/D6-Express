using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

namespace _Main.Scripts.UI
{
	public class UIPlayerHud : UIBaseElement
	{
		[SerializeField]
		private TextMeshProUGUI cashCountText;

		public void SetCashCountText(string text)
		{
			cashCountText.text = text;
		}
	}
}
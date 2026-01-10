using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

public class UIHintView : UIBaseElement
{
	[SerializeField]
	private TextMeshProUGUI hintText;

	public void SetHintText(string text)
	{
		hintText.text = text;
	}
}
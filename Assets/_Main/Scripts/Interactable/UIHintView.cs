using _Main.Scripts.UI;
using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

public class UIHintView : UIBaseElement
{
	[SerializeField]
	private TextMeshProUGUI hintText;
	
	[SerializeField]
	private TextMeshProUGUI inputText;
	
	[SerializeField]
	private TextStyleRef defaultTextStyle;

	[SerializeField]
	private TextStyleRef highlightedTextStyle;

	[SerializeField]
	private TextStyleRef defaultHintStyle;

	[SerializeField]
	private TextStyleRef highlightedHintStyle;


	public void SetHintText(string text, bool highlighted = false)
	{
		hintText.text = text;
		hintText.color = highlighted ? highlightedTextStyle.Color : defaultTextStyle.Color;
		inputText.color = highlighted ? highlightedHintStyle.Color : defaultHintStyle.Color;
	}
}
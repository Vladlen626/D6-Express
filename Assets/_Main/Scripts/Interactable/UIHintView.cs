using _Main.Scripts.UI;
using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHintView : UIBaseElement
{
	[SerializeField]
	private TextMeshProUGUI hintText;
	
	[SerializeField]
	private TextMeshProUGUI inputText;
	
	[SerializeField]
	private Image hintBg;

	[SerializeField]
	private Image inputBg;
	
	[SerializeField]
	private TextStyleRef defaultTextStyle;

	[SerializeField]
	private TextStyleRef highlightedTextStyle;

	[SerializeField]
	private TextStyleRef defaultHintStyle;

	[SerializeField]
	private TextStyleRef highlightedHintStyle;
	
	[SerializeField]
	private ColorStyleRef defaultBgTextStyle;

	[SerializeField]
	private ColorStyleRef highlightedBgTextStyle;
	
	[SerializeField]
	private ColorStyleRef bgHintStyle;

	[SerializeField]
	private ColorStyleRef highlightedBgHintStyle;


	public void SetHintText(string text, bool highlighted = false)
	{
		hintText.text = text;
		hintText.color = highlighted ? highlightedTextStyle.Color : defaultTextStyle.Color;
		inputText.color = highlighted ? highlightedHintStyle.Color : defaultHintStyle.Color;
		hintBg.color = highlighted ? highlightedBgTextStyle.Value : defaultBgTextStyle.Value;
		inputBg.color = highlighted ? highlightedBgHintStyle.Value : bgHintStyle.Value;
	}
}
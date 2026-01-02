using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

public class UISpeechView : UIBaseElement
{
	[SerializeField]
	private GameObject speechPanel;

	[SerializeField]
	private TextMeshProUGUI speechText;

	public void SetSpeech(string text)
	{
		// todo уебищная эвристика
		speechPanel.SetActive(text != string.Empty);
		speechText.text = text;
	}
}
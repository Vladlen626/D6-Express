using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

public class UISpeechView : UIBaseElement
{
	[SerializeField]
	private GameObject speechPanel;

	[SerializeField]
	private TextMeshProUGUI speechText;

	[SerializeField]
	private TextMeshProUGUI speakerNameText;

	public void SetSpeakerName(string text)
	{
		// todo уебищная эвристика
		speechPanel.SetActive(text != string.Empty);
		speakerNameText.text = text;
	}

	public void SetSpeech(string text)
	{
		// todo уебищная эвристика
		speechPanel.SetActive(text != string.Empty);
		speechText.text = text;
	}
}
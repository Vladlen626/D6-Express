using PlatformCore.Services.UI;
using TMPEffects.Components;
using TMPro;
using UnityEngine;

public class UISpeechView : UIBaseElement
{
	[SerializeField]
	private GameObject speechPanel;

	[SerializeField]
	private GameObject choiceOptions;

	[SerializeField]
	private GameObject hint;

	[SerializeField]
	private TextMeshProUGUI speechText;

	[SerializeField]
	private TextMeshProUGUI speakerNameText;

	[SerializeField]
	private TMPWriter TMPWriter;

	[SerializeField]
	private UIBackgroundSizer speakerNameSizer;

	public void SetSpeakerName(string text)
	{
		// todo уебищная эвристика
		speechPanel.SetActive(text != string.Empty);
		speakerNameText.text = text;
		if (speakerNameSizer)
		{
			speakerNameSizer.Refresh();
		}
	}

	public void SetSpeech(string text)
	{
		// todo уебищная эвристика
		speechPanel.SetActive(text != string.Empty);
		speechText.text = text;
	}

	public void ShowChoiceOptions()
	{
		choiceOptions.SetActive(true);
		hint.SetActive(false);
	}

	public void HideChoiceOptions()
	{
		choiceOptions.SetActive(false);
		hint.SetActive(true);
	}

	public bool IsWriting()
	{
		return TMPWriter.IsWriting;
	}

	public void SkipWriter(bool showAnimation = true)
	{
		TMPWriter.SkipWriter(showAnimation);
	}
}

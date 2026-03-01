using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

public class UISpeechView : UIBaseElement
{
	[SerializeField]
	private GameObject speechPanel;

	[SerializeField]
	private GameObject choiceOptions;

	[SerializeField]
	private TextMeshProUGUI speechText;

	[SerializeField]
	private TextMeshProUGUI speakerNameText;

	[SerializeField]
	private UIBackgroundSizer speakerNameSizer;

	protected override void OnAwake()
	{
		base.OnAwake();
		if (!speakerNameSizer && speakerNameText)
		{
			speakerNameSizer = speakerNameText.GetComponentInParent<UIBackgroundSizer>();
		}
	}

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
	}

	public void HideChoiceOptions()
	{
		choiceOptions.SetActive(false);
	}
}

using System;
using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStatsView : UIBaseElement
{
	[SerializeField] private TextMeshProUGUI locationName;

	[SerializeField] private UIModifiersView uIModifiersView;

	[SerializeField] private Button startButton;

	[SerializeField] private LocalizedText daysRemaining;

	public UIModifiersView UIModifiersView => uIModifiersView;

	public event Action StartButtonClicked;

	private void OnEnable()
	{
		startButton.onClick.AddListener(() =>
		{
			StartButtonClicked?.Invoke();
		});
	}

	private void OnDisable()
	{
		startButton.onClick.RemoveAllListeners();
	}

	public void SetMessage(string name)
	{
		locationName.text = name;
	}

    public void SetDaysRemaningText(string id, params string[] args)
    {
        daysRemaining.SetText(id, args);
    }
}
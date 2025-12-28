using System;
using _Main.Scripts.Game.Views;
using TMPro;
using UnityEngine;

public class DiceTableView : MonoBehaviour
{
	[SerializeField] private DicePositionsHandler dicePositionsHandler;

	public DicePositionsHandler DicePositionsHandler => dicePositionsHandler;

	[Header("Score")]
	[SerializeField] private TextMeshPro targetScoreText;
	[SerializeField] private TextMeshPro bankedScoreText;
	[SerializeField] private TextMeshPro currentScoreText;
	[SerializeField] private TextMeshPro previewScoreText;


	[Header("Buttons")]
	[SerializeField] private ButtonView rollButton;
	[SerializeField] private ButtonView saveButton;
	[SerializeField] private ButtonView passButton;

	public event Action OnRollClicked;
	public event Action OnSaveClicked;
	public event Action OnPassClicked;

	private void Start()
	{
		rollButton.OnClicked += () => OnRollClicked?.Invoke();
		saveButton.OnClicked += () => OnSaveClicked?.Invoke();
		passButton.OnClicked += () => OnPassClicked?.Invoke();
	}

	public void SetButtonInteractable(string buttonName, bool interactable)
	{
		switch (buttonName)
		{
			case "Roll":
				rollButton.SetInteractable(interactable);
				break;
			case "Save":
				saveButton.SetInteractable(interactable);
				break;
			case "Pass":
				passButton.SetInteractable(interactable);
				break;
		}
	}

	public void SetBankedPointsText(string text)
	{
		bankedScoreText.text = text;
	}

	public void SetTargetPointsText(string text)
	{
		targetScoreText.text = text;
	}

	public void SetCurrentPointsText(string text)
	{
		currentScoreText.text = text;
	}

	public void SetPreviewPointsText(string text)
	{
		previewScoreText.text = text;
	}
}
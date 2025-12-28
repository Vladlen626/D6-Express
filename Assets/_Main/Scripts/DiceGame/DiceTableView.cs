using System;
using _Main.Scripts.Game.Views;
using TMPro;
using UnityEngine;

public class DiceTableView : MonoBehaviour
{
	[SerializeField] private DicePositionsHandler dicePositionsHandler;

	public DicePositionsHandler DicePositionsHandler => dicePositionsHandler;

	[Header("Score")] [SerializeField] private TextMeshProUGUI targetScoreText;
	[SerializeField] private TextMeshProUGUI totalScoreText;
	[SerializeField] private TextMeshProUGUI currentScoreText;


	[Header("Buttons")] [SerializeField] private ButtonView rollButton;
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

	public void SetTotalScoreText(string text)
	{
		totalScoreText.text = text;
	}

	public void SetTargetScoreText(string text)
	{
		targetScoreText.text = text;
	}

	public void SetCurrentScoreText(string text)
	{
		currentScoreText.text = text;
	}
}
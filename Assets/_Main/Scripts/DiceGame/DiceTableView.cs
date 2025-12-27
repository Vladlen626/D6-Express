using System;
using _Main.Scripts.Game.Views;
using UnityEngine;

public class DiceTableView : MonoBehaviour
{
	[SerializeField] private DicePositionsHandler dicePositionsHandler;

	public DicePositionsHandler DicePositionsHandler => dicePositionsHandler;

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
}
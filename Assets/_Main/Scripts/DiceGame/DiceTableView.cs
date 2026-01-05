using System;
using _Main.Scripts.Game.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiceTableView : MonoBehaviour
{
	public event Action<int> OnBetSliderChange; 
	public event Action OnRollClicked;
	public event Action OnPassClicked;
	public event Action OnBetClicked;
	public event Action OnPlayClicked;
	
	[Header("StateHandlers")]
	[SerializeField] private Transform gameStateHandler;
	[SerializeField] private Transform betStateHandler;
	[SerializeField] private Transform diceSelectionHandler;
	
	[Header("Turn")]
	[SerializeField] private TextMeshPro turnText;

	[Header("Score")]
	[SerializeField] private TextMeshPro targetScoreText;
	[SerializeField] private TextMeshPro bankedScoreText;
	[SerializeField] private TextMeshPro currentScoreText;
	[SerializeField] private TextMeshPro previewScoreText;

	[Header("Buttons")]
	[SerializeField] private ButtonView rollButton;
	[SerializeField] private ButtonView passButton;
	[SerializeField] private ButtonView betButton;
	[SerializeField] private ButtonView playButton;

	[Header("Bet")]
	[SerializeField] private Slider betSlider;
	[SerializeField] private TextMeshPro currentBetText;
	[SerializeField] private TextMeshPro minBetText;
	[SerializeField] private TextMeshPro maxBetText;

	[SerializeField] private DicePositionsHandler gameStatePosHandler;
	[SerializeField] private DicePositionsHandler selectionStatePosHandler;
	public DicePositionsHandler GameStatePosHandler => gameStatePosHandler;
	public DicePositionsHandler SelectionStatePosHandler => selectionStatePosHandler;

	private void Start()
	{
		rollButton.OnClicked += () => OnRollClicked?.Invoke();
		passButton.OnClicked += () => OnPassClicked?.Invoke();
		betButton.OnClicked += () => OnBetClicked?.Invoke();
		betButton.OnClicked += () => OnPlayClicked?.Invoke();
		betSlider.onValueChanged.AddListener(OnSliderChanged);
	}

	private void OnDestroy()
	{
		betSlider.onValueChanged.RemoveAllListeners();
	}
	
	private void OnSliderChanged(float value)
	{
		OnBetSliderChange?.Invoke((int)value);
	}

	public void SetButtonInteractable(string buttonName, bool interactable)
	{
		switch (buttonName)
		{
			case "Roll":
				rollButton.SetInteractable(interactable);
				break;
			case "Pass":
				passButton.SetInteractable(interactable);
				break;
		}
	}

	public void SwitchGameStateView(DiceGameState state)
	{
		gameStateHandler.gameObject.SetActive(false);
		betStateHandler.gameObject.SetActive(false);
		
		switch (state)
		{
			case DiceGameState.GAME:
				gameStateHandler.gameObject.SetActive(true);
				break;
			case DiceGameState.BET:
				betStateHandler.gameObject.SetActive(true);
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

	public void SetTurnText(int currentTurn, int maxTurn)
	{
		turnText.text = $"Turns: {currentTurn}/{maxTurn}";
	}

	public void SetCurrentBetText(string text)
	{
		currentBetText.text = text;
	}

	public void SetBet(int bet)
	{
		betSlider.value = bet;
	}

	public void SetMinBet(int minBet)
	{
		minBetText.text = minBet.ToString();
		betSlider.minValue = minBet;
	}

	public void SetMaxBet(int MaxBet)
	{
		maxBetText.text = MaxBet.ToString();
		betSlider.maxValue = MaxBet;
	}
}
using System;
using _Main.Scripts.Core;
using _Main.Scripts.Game.Views;
using _Main.Scripts.UI;
using DG.Tweening;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class DiceTableView : MonoBehaviour
{
	public event Action<int> OnBetSliderChange; 
	public event Action OnRollClicked;
	public event Action OnPassClicked;
	public event Action OnBetClicked;
	public event Action OnPlayClicked;
	
	[SerializeField] private CinemachineCamera cinemachineCamera;
	[SerializeField] private Transform diceCombinationsTransform;
	
	[Header("StateHandlers")]
	[SerializeField] private Transform gameStateHandler;
	[SerializeField] private Transform betStateHandler;
	[SerializeField] private Transform selectStateHandler;

	[Header("Turn")] 
	[SerializeField] private TextMeshPro turnText;
	[SerializeField] private TextMeshPro turnOwnerText;

	[Header("Score")]
	[SerializeField] private TextMeshPro targetScoreText;
	[SerializeField] private TextMeshPro bankedScoreText;
	[SerializeField] private TextMeshPro enemyBankedScoreText;
	[SerializeField] private TextMeshPro currentScoreText;
	[SerializeField] private TextMeshPro previewScoreText;
	[SerializeField] private TextMeshPro previewScoreText2;

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
	
	[SerializeField] private float animDuration = 0.15f;
	public DicePositionsHandler GameStatePosHandler => gameStatePosHandler;
	public DicePositionsHandler SelectionStatePosHandler => selectionStatePosHandler;
	public CinemachineCamera TableCamera => cinemachineCamera;

	private void Awake()
	{
		DisableCamera();
	}

	private void Start()
	{
		rollButton.OnClicked += () => OnRollClicked?.Invoke();
		passButton.OnClicked += () => OnPassClicked?.Invoke();
		betButton.OnClicked += () => OnBetClicked?.Invoke();
		playButton.OnClicked += () => OnPlayClicked?.Invoke();
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

	//Todo: когда-то здесь будут не строки, обязательно....
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
			case "Play":
				playButton.SetInteractable(interactable);
				break;
		}
	}

	public void SwitchGameStateView(DiceGameState state)
	{
		gameStateHandler.gameObject.SetActive(false);
		betStateHandler.gameObject.SetActive(false);
		selectStateHandler.gameObject.SetActive(false);
		
		switch (state)
		{
			case DiceGameState.GAME:
				gameStateHandler.gameObject.SetActive(true);
				break;
			case DiceGameState.BET:
				betStateHandler.gameObject.SetActive(true);
				break;
			case DiceGameState.SELECT_DICE:
				selectStateHandler.gameObject.SetActive(true);
				break;
		}
	}

	public void SwitchTurn(bool isPlayerTurn)
	{
		turnOwnerText.text = isPlayerTurn ? "Your Turn" : "Enemy Turn";
		turnOwnerText.color = isPlayerTurn ? Color.blue : Color.red;
		passButton.gameObject.SetActive(isPlayerTurn);
		rollButton.gameObject.SetActive(isPlayerTurn);
	}

	public void EnableCamera()
	{
		cinemachineCamera.gameObject.SetActive(true);
	}

	public void DisableCamera()
	{
		cinemachineCamera.gameObject.SetActive(false);
	}

	public void SetPlayerBankedPointsText(int oldValue, int newValue)
	{
		UIUtils.UpdateUiIntValueText(bankedScoreText, oldValue, newValue, v => v.ToString());
	}

	public void SetEnemyBankedPointsText(int oldValue, int newValue)
	{
		UIUtils.UpdateUiIntValueText(enemyBankedScoreText, oldValue, newValue, v => v.ToString());
	}

	public void SetTargetPointsText(int oldValue, int newValue)
	{
		UIUtils.UpdateUiIntValueText(targetScoreText, oldValue, newValue, v => $"Target: {v}");
	}

	public void SetCurrentPointsText(int oldValue, int newValue)
	{
		UIUtils.UpdateUiIntValueText(currentScoreText, oldValue, newValue, v => v.ToString());
	}

	public void SetPreviewPointsText(int oldValue, int newValue)
	{
		UIUtils.UpdateUiIntValueText(previewScoreText, oldValue, newValue, v => v.ToString());
		UIUtils.UpdateUiIntValueText(previewScoreText2, oldValue, newValue, v => v.ToString());
	}

	public void SetTurnText(int oldValue, int newValue)
	{
		UIUtils.UpdateUiIntValueText(turnText, oldValue, newValue, v => $"Turn: {v}");
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

	public void DiceCombinationsToggle()
	{
		if (Mathf.Approximately(diceCombinationsTransform.localScale.z, 1))
		{
			diceCombinationsTransform.DOScaleZ(0, animDuration)
				.OnComplete(() => diceCombinationsTransform.gameObject.SetActive(false));
		} else if (diceCombinationsTransform.localScale.z == 0)
		{
			diceCombinationsTransform.gameObject.SetActive(true);
			diceCombinationsTransform.DOScaleZ(1, animDuration);
		}
	}


}
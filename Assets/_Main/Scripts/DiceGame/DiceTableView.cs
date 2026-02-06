using System;
using _Main.Scripts.Dice;
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

	[Header("Global")]
	[SerializeField] private Transform combosTransform;
	[SerializeField] private Transform tooltipPos;

	[Header("StateHandlers")]
	[SerializeField] private Transform gameStateHandler;
	[SerializeField] private Transform betStateHandler;
	[SerializeField] private Transform selectStateHandler;

	[Header("Turn")]
	[SerializeField] private TextMeshProUGUI turnText;
	[SerializeField] private TurnChipView _turnChipView;
	[SerializeField] private Transform playerBankPlane;
	[SerializeField] private Transform enemyBankPlane;

	[Header("Score")]
	[SerializeField] private TextMeshProUGUI targetScoreText;
	[SerializeField] private TextMeshProUGUI bankedScoreText;
	[SerializeField] private TextMeshProUGUI enemyBankedScoreText;
	[SerializeField] private TextMeshProUGUI comboNameText;
	[SerializeField] private TextMeshProUGUI turnScoreText;
	[SerializeField] private TextMeshProUGUI previewScoreText;

	[Header("Buttons")]
	[SerializeField] private Button rollButton;
	[SerializeField] private Button passButton;
	[SerializeField] private Button betButton;
	[SerializeField] private Button playButton;

	[Header("Bet")]
	[SerializeField] private Slider betSlider;
	[SerializeField] private TextMeshPro currentBetText;
	[SerializeField] private TextMeshPro minBetText;
	[SerializeField] private TextMeshPro maxBetText;

	[SerializeField] private DicePositionsHandler gameStatePosHandler;
	[SerializeField] private DicePositionsHandler selectionStatePosHandler;

	[Header("Items")]
	[SerializeField] private DiceItemView itemViewPrefab;
	[SerializeField] private Transform[] itemSlotsSelection;
	[SerializeField] private Transform[] itemSlotsGame;

	[Header("Items")]
	[SerializeField] private CombinationsView combinationsView;

	[SerializeField] private float animDuration = 0.15f;
	public DicePositionsHandler GameStatePosHandler => gameStatePosHandler;
	public DicePositionsHandler SelectionStatePosHandler => selectionStatePosHandler;
	public CombinationsView CombinationsView => combinationsView;
	public DiceItemView ItemViewPrefab => itemViewPrefab;
	public Transform[] ItemSlotsSelection => itemSlotsSelection;
	public Transform[] ItemSlotsGame => itemSlotsGame;
	public Transform TooltipPos => tooltipPos;

	private bool isCombinationsOpen;
	private bool inAnimProcess;

	private void Start()
	{
		rollButton.onClick.AddListener(() => OnRollClicked?.Invoke());
		passButton.onClick.AddListener(() => OnPassClicked?.Invoke());
		betButton.onClick.AddListener(() => OnBetClicked?.Invoke());
		playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());
		betSlider.onValueChanged.AddListener(OnSliderChanged);
	}

	private void OnDestroy()
	{
		rollButton.onClick.RemoveAllListeners();
		passButton.onClick.RemoveAllListeners();
		betButton.onClick.RemoveAllListeners();
		playButton.onClick.RemoveAllListeners();
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
				rollButton.interactable = interactable;
				break;
			case "Pass":
				passButton.interactable = interactable;
				break;
			case "Play":
				playButton.interactable = interactable;
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
		_turnChipView.SwitchTurn(isPlayerTurn);

		playerBankPlane.gameObject.SetActive(isPlayerTurn);
		enemyBankPlane.gameObject.SetActive(!isPlayerTurn);

		passButton.gameObject.SetActive(isPlayerTurn);
		rollButton.gameObject.SetActive(isPlayerTurn);
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
		UIUtils.UpdateUiIntValueText(turnScoreText, oldValue, newValue, v => v.ToString());
	}

	public void SetComboNameText(string id)
	{
		comboNameText.text = id;
	}

	public void SetPreviewPointsText(int oldValue, int newValue)
	{
		UIUtils.UpdateUiIntValueText(previewScoreText, oldValue, newValue, v => v.ToString());
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
		return;
		if (inAnimProcess)
		{
			return;
		}

		if (!isCombinationsOpen)
		{
			inAnimProcess = true;
			combosTransform.DOLocalRotate(Vector3.forward * 90, animDuration)
				.OnComplete(() =>
				{
					isCombinationsOpen = true;
					inAnimProcess = false;
				});
		}
		else
		{
			inAnimProcess = true;
			combosTransform.DOLocalRotate(Vector3.zero, animDuration)
				.OnComplete(() =>
				{
					isCombinationsOpen = false;
					inAnimProcess = false;
				});
		}
	}


}

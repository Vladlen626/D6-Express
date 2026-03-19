using System;
using _Main.Scripts.Dice;
using _Main.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DiceTableView : MonoBehaviour
{
	private const float DefaultUpgradeDiceScale = 1f;

	public event Action OnPlayRequested;
	public event Action OnPlayAllowed;
	public event Action OnRollClicked;
	public event Action OnPassClicked;
	public event Action OnPlayClicked;
	public event Action OnBet1xClicked;
	public event Action OnBet3xClicked;
	public event Action OnBet5xClicked;
	public event Action OnAllInClicked;

	[Header("Global")]
	[SerializeField]
	private Transform[] DiceBonusPositions;
	[SerializeField]
	private Transform[] SelectDiceBonusPositions;

	[Header("StateHandlers")]
	[SerializeField] private Transform defaultStateHandler;
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

	[Header("Combinations Cards")]
	[SerializeField] private Transform combinationCardsRoot;
	[SerializeField] private DiceCombinationCardView combinationCardPrefab;
	[SerializeField] private RectTransform combinationFlyLayer;
	[SerializeField] private TextMeshProUGUI combinationFlyScorePrefab;
	[SerializeField] private RectTransform turnScoreFlyTarget;

	[Header("Buttons")]
	[SerializeField] private Button rollButton;
	[SerializeField] private Button passButton;
	[SerializeField] private Button playButton;

	[Header("Bet")]
	[SerializeField] private string betPrefix = "$";
	[SerializeField] private Button bet1xButton;
	[SerializeField] private Button bet3xButton;
	[SerializeField] private Button bet5xButton;
	[SerializeField] private Button allInButton;
	[SerializeField] private Transform betMultipliersRoot;
	[SerializeField] private Transform allInRoot;
	[SerializeField] private TMP_Text bet1xLabelText;
	[SerializeField] private TMP_Text bet3xLabelText;
	[SerializeField] private TMP_Text bet5xLabelText;
	[SerializeField] private TMP_Text allInLabelText;
	[SerializeField] private CouplePositionsHandler gameStatePosHandler;
	[SerializeField] private CouplePositionsHandler selectionStatePosHandler;

	[Header("Items")]
	[SerializeField] private ItemView itemViewPrefab;
	[SerializeField] private Transform[] itemSlotsSelection;
	[SerializeField] private Transform[] itemSlotsGame;
	[SerializeField] private DiceItemTargetingView itemTargetingView;
	[SerializeField] private int itemTargetingLockedArrowsPoolSize = 3;

	[Header("Upgrade")]
	[SerializeField] private Transform upgradeDicePos;
	[SerializeField] private float upgradeDiceScreenScale = 1f;

	[SerializeField] private float animDuration = 0.15f;
	public CouplePositionsHandler GameStatePosHandler => gameStatePosHandler;
	public CouplePositionsHandler SelectionStatePosHandler => selectionStatePosHandler;
	public ItemView ItemViewPrefab => itemViewPrefab;
	public Transform[] ItemSlotsSelection => itemSlotsSelection;
	public Transform[] ItemSlotsGame => itemSlotsGame;
	public DiceItemTargetingView ItemTargetingView => itemTargetingView;
	public int ItemTargetingLockedArrowsPoolSize => itemTargetingLockedArrowsPoolSize;
	public Transform[] DiceBonusSlots => DiceBonusPositions;
	public Transform[] SelectDiceBonusSlots => SelectDiceBonusPositions;
	public Transform UpgradeDicePos => upgradeDicePos;
	public float UpgradeDiceScreenScale => upgradeDiceScreenScale > 0.05f ? upgradeDiceScreenScale : DefaultUpgradeDiceScale;
	public Transform CombinationCardsRoot => combinationCardsRoot;
	public DiceCombinationCardView CombinationCardPrefab => combinationCardPrefab;
	public RectTransform CombinationFlyLayer => combinationFlyLayer;
	public TextMeshProUGUI CombinationFlyScorePrefab => combinationFlyScorePrefab;
	public RectTransform TurnScoreFlyTarget => turnScoreFlyTarget;

	private bool isCombinationsOpen;
	private bool inAnimProcess;

	private void Start()
	{
		ValidateBetReferences();
		rollButton.onClick.AddListener(() => OnRollClicked?.Invoke());
		passButton.onClick.AddListener(() => OnPassClicked?.Invoke());
		playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());
		bet1xButton.onClick.AddListener(() => OnBet1xClicked?.Invoke());
		bet3xButton.onClick.AddListener(() => OnBet3xClicked?.Invoke());
		bet5xButton.onClick.AddListener(() => OnBet5xClicked?.Invoke());
		allInButton.onClick.AddListener(() => OnAllInClicked?.Invoke());
	}

	private void OnDestroy()
	{
		rollButton.onClick.RemoveAllListeners();
		passButton.onClick.RemoveAllListeners();
		playButton.onClick.RemoveAllListeners();
		bet1xButton.onClick.RemoveAllListeners();
		bet3xButton.onClick.RemoveAllListeners();
		bet5xButton.onClick.RemoveAllListeners();
		allInButton.onClick.RemoveAllListeners();
	}

	public void RequestPlay()
	{
		OnPlayRequested?.Invoke();
	}

	public void AllowPlay()
	{
		OnPlayAllowed?.Invoke();
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
		defaultStateHandler.gameObject.SetActive(false);
		gameStateHandler.gameObject.SetActive(false);
		betStateHandler.gameObject.SetActive(false);
		selectStateHandler.gameObject.SetActive(false);

		switch (state)
		{
			case DiceGameState.DEFAULT:
				defaultStateHandler.gameObject.SetActive(true);
				break;
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
		UIUtils.UpdateUiIntValueText(bankedScoreText, oldValue, newValue, "{0:0}", animDuration);
	}

	public void SetEnemyBankedPointsText(int oldValue, int newValue)
	{
		UIUtils.UpdateUiIntValueText(enemyBankedScoreText, oldValue, newValue, "{0:0}", animDuration);
	}

	public void SetTargetPointsText(int oldValue, int newValue)
	{
		UIUtils.UpdateUiIntValueText(targetScoreText, oldValue, newValue, "Target: {0:0}", animDuration);
	}

	public void SetCurrentPointsText(int oldValue, int newValue)
	{
		UIUtils.UpdateUiIntValueText(turnScoreText, oldValue, newValue, "{0:0}", animDuration);
	}

	public void SetComboNameText(string id)
	{
		comboNameText.text = id;
	}

	public void SetPreviewPointsText(int oldValue, int newValue)
	{
		UIUtils.UpdateUiIntValueText(previewScoreText, oldValue, newValue, "{0:0}", animDuration);
	}

	public void SetTurnText(int oldValue, int newValue)
	{
		UIUtils.UpdateUiIntValueText(turnText, oldValue, newValue, "Turn: {0:0}", animDuration);
	}

	public void SetDiceBonusPositionVisibility(bool visible)
	{
		for (int i = 0; i < DiceBonusPositions.Length; i++)
		{
			DiceBonusPositions[i].gameObject.SetActive(visible);
		}
	}

	public void SetSelectDiceBonusPositionVisibility(bool visible)
	{
		for (int i = 0; i < SelectDiceBonusPositions.Length; i++)
		{
			SelectDiceBonusPositions[i].gameObject.SetActive(visible);
		}
	}

	public void ShowBetMultipliers(bool visible)
	{
		betMultipliersRoot.gameObject.SetActive(visible);
	}

	public void ShowAllInButton(bool visible)
	{
		allInRoot.gameObject.SetActive(visible);
	}

	public void SetBetMultiplierButtonsInteractable(bool can1x, bool can3x, bool can5x)
	{
		bet1xButton.interactable = can1x;
		bet3xButton.interactable = can3x;
		bet5xButton.interactable = can5x;
	}

	public void SetAllInButtonInteractable(bool interactable)
	{
		allInButton.interactable = interactable;
	}

	public void SetBetButtonsAmounts(int bet1x, int bet3x, int bet5x, int allInBet)
	{
		bet1xLabelText.text = FormatBetLabel(bet1x);
		bet3xLabelText.text = FormatBetLabel(bet3x);
		bet5xLabelText.text = FormatBetLabel(bet5x);
		allInLabelText.text = $"ALL IN: {FormatBetLabel(allInBet)}";
	}

	private string FormatBetLabel(int amount)
	{
		return string.IsNullOrEmpty(betPrefix) ? amount.ToString() : $"{betPrefix}{amount}";
	}

	private void ValidateBetReferences()
	{
		if (!bet1xButton || !bet3xButton || !bet5xButton || !allInButton || !betMultipliersRoot || !allInRoot ||
			bet1xLabelText == null || bet3xLabelText == null || bet5xLabelText == null || allInLabelText == null)
		{
			throw new MissingReferenceException("[DiceTableView] BET controls are not configured. Assign bet buttons, roots and label texts in prefab.");
		}
	}

	public void ValidateCombinationCardReferences()
	{
		if (!combinationCardsRoot || !combinationCardPrefab || !combinationFlyLayer || !combinationFlyScorePrefab || !turnScoreFlyTarget)
		{
			throw new MissingReferenceException(
				"[DiceTableView] Combination card UI is not configured. Assign cards root, card prefab, fly layer, fly label prefab and turn score fly target.");
		}
	}
}

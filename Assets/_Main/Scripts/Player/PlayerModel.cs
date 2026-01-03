using System;

public class PlayerModel
{
	private LevelModel levelModel;

	public InventoryModel InventoryModel { get; private set; }
	public CharacterState currentCharacterState { get; private set; }

	public event Action<CharacterState, CharacterState> OnCharacterStateChanged;

	public PlayerModel()
	{
		InventoryModel = new InventoryModel();
	}

	public void Init(LevelModel levelModel)
	{
		this.levelModel = levelModel;
		this.levelModel.LevelStateChanged += OnLevelStateChanged;
	}

	public void SetCharacterState(CharacterState characterState)
	{
		var oldCharacterState = currentCharacterState;
		currentCharacterState = characterState;
		OnCharacterStateChanged?.Invoke(oldCharacterState, currentCharacterState);
	}

	private void OnLevelStateChanged()
	{
		SetCharacterState(CharacterState.LOCATION_TRANSITIONING);
	}
}

public class InventoryModel
{
	public event Action OnCashCountChanged;
	public int CashCount => cashCount;

	private int cashCount;

	public void GiveCash(int amount)
	{
		cashCount += amount;
		OnCashCountChanged?.Invoke();
	}

	public void TakeCash(int amount)
	{
		cashCount -= amount;
		OnCashCountChanged?.Invoke();
	}

	public void SetCash(int amount)
	{
		cashCount = amount;
		OnCashCountChanged?.Invoke();
	}
}


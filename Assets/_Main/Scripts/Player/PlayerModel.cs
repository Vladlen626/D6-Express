using System;

public class PlayerModel
{
	public event Action OnCharacterStateChanged;
	public InventoryModel InventoryModel {get; private set;}
	public CharacterState currentCharacterState { get; private set; }

	public PlayerModel()
	{
		InventoryModel = new InventoryModel();
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
}


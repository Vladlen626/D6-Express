using System;

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
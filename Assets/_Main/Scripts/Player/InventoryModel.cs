using System;
using System.Collections.Generic;

public class InventoryModel
{
	public event Action OnCashCountChanged;
	public int CashCount => cashCount;
	public IReadOnlyList<string> DiceIdList => diceIdList;

	private readonly List<string> diceIdList = new();
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

	public void AddDice(string diceId)
	{
		diceIdList.Add(diceId);
	}

	public void RemoveDice(string diceId)
	{
		diceIdList.Remove(diceId);
	}

	public void RemoveAllDices()
	{
		diceIdList.Clear();
	}
}
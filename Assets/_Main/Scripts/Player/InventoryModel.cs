using System;
using System.Collections.Generic;
using _Main.Scripts.Dice;

public class InventoryModel
{
	private readonly List<string> diceIdList = new();
	private int cashCount;

	public int CashCount => cashCount;
	public IReadOnlyList<string> DiceIdList => diceIdList;
	public ModifiersModel ModifiersModel { get; }
	public ItemsModel ItemsModel { get; }
	public event Action ItemsChanged;

	public event Action OnCashCountChanged;
	public event Action<string> DiceAdded;
	public event Action<string> DiceRemoved;

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
		DiceAdded?.Invoke(diceId);
	}

	public void RemoveDice(string diceId)
	{
		diceIdList.Remove(diceId);
		DiceRemoved?.Invoke(diceId);
	}

	public void RemoveAllDices()
	{
		foreach (var item in diceIdList)
		{
			DiceRemoved?.Invoke(item);
		}
		diceIdList.Clear();
	}

	public InventoryModel()
	{
		ModifiersModel = new ModifiersModel();
		ItemsModel = new ItemsModel(ModifiersModel);
		ItemsModel.ItemsChanged += () => ItemsChanged?.Invoke();
	}
}

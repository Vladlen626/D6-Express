using System;
using System.Collections.Generic;
using _Main.Scripts.Dice;

public class InventoryModel
{
	private readonly List<string> diceIdList = new();
	private readonly List<string> modifierItemIds = new();
	private int cashCount;

	public int CashCount => cashCount;
	public IReadOnlyList<string> DiceIdList => diceIdList;
	public IReadOnlyList<string> ModifierItemIds => modifierItemIds;
	public ModifiersModel ModifiersModel { get; }
	public ModifierItemsModel ModifierItemsModel { get; }
	public event Action ItemsChanged;

	public event Action OnCashCountChanged;
	public event Action<string> DiceAdded;
	public event Action<string> DiceRemoved;
	public event Action<string> ModifierItemAdded;
	public event Action<string> ModifierItemRemoved;

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

	public void AddModifierItem(string itemId)
	{
		if (string.IsNullOrEmpty(itemId))
		{
			return;
		}

		if (modifierItemIds.Contains(itemId))
		{
			return;
		}

		modifierItemIds.Add(itemId);
		ModifierItemAdded?.Invoke(itemId);
	}

	public void RemoveModifierItem(string itemId)
	{
		if (string.IsNullOrEmpty(itemId))
		{
			return;
		}

		if (modifierItemIds.Remove(itemId))
		{
			ModifierItemRemoved?.Invoke(itemId);
		}
	}

	public void RemoveAllModifierItems()
	{
		foreach (var item in modifierItemIds)
		{
			ModifierItemRemoved?.Invoke(item);
		}
		modifierItemIds.Clear();
	}

	public InventoryModel()
	{
		ModifiersModel = new ModifiersModel();
		ModifierItemsModel = new ModifierItemsModel(ModifiersModel);
		ModifierItemsModel.ItemsChanged += () => ItemsChanged?.Invoke();
	}
}

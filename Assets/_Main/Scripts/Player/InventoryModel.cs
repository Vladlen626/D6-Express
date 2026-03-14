using System;
using System.Collections.Generic;
using _Main.Scripts.Dice;

public class InventoryModel
{
	private const int DefaultModifierItemsCapacity = 6;

	private readonly List<string> diceIdList = new();
	private readonly List<string> modifierItemIds = new();
	private int cashCount;
	private int modifierItemsCapacity = DefaultModifierItemsCapacity;

	public int CashCount => cashCount;
	public IReadOnlyList<string> DiceIdList => diceIdList;
	public IReadOnlyList<string> ModifierItemIds => modifierItemIds;
	public int ModifierItemsCapacity => modifierItemsCapacity;
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
		TryAddModifierItem(itemId);
	}

	public ModifierItemAddResult ValidateModifierItemAdd(string itemId)
	{
		if (string.IsNullOrEmpty(itemId))
		{
			return ModifierItemAddResult.InvalidId;
		}

		if (modifierItemIds.Contains(itemId))
		{
			return ModifierItemAddResult.Duplicate;
		}

		if (modifierItemIds.Count >= modifierItemsCapacity)
		{
			return ModifierItemAddResult.InventoryFull;
		}

		return ModifierItemAddResult.Success;
	}

	public ModifierItemAddResult TryAddModifierItem(string itemId)
	{
		var result = ValidateModifierItemAdd(itemId);
		if (result != ModifierItemAddResult.Success)
		{
			return result;
		}

		modifierItemIds.Add(itemId);
		ModifierItemAdded?.Invoke(itemId);
		return ModifierItemAddResult.Success;
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

	public void SetModifierItemsCapacity(int value)
	{
		if (value < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(value), value,
				"[InventoryModel] Modifier items capacity cannot be negative.");
		}

		if (value < modifierItemIds.Count)
		{
			throw new InvalidOperationException(
				$"[InventoryModel] Cannot set modifier items capacity to {value} because inventory already contains {modifierItemIds.Count} items.");
		}

		modifierItemsCapacity = value;
	}

	public InventoryModel()
	{
		ModifiersModel = new ModifiersModel();
		ModifierItemsModel = new ModifierItemsModel(ModifiersModel);
		ModifierItemsModel.ItemsChanged += () => ItemsChanged?.Invoke();
	}
}

public enum ModifierItemAddResult
{
	Success = 0,
	InvalidId = 1,
	Duplicate = 2,
	InventoryFull = 3
}

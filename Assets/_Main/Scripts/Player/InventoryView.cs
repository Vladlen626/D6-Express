using System.Collections.Generic;
using TMPro;
using _Main.Scripts.Dice;
using UnityEngine;

/// <summary>
/// Simple UI view that mirrors the player's inventory contents into two slot groups:
/// dice (by id) and modifier items (by display name). Text meshes are created lazily
/// under the provided slot transforms.
/// </summary>
public class InventoryView : MonoBehaviour
{
	[Header("Slots")]
	[SerializeField] private Transform[] diceSlots;
	[SerializeField] private Transform[] itemSlots;

	[Header("Texts")]
	[SerializeField] private TextMeshProUGUI cashText;
	[SerializeField] private TextMeshProUGUI diceCountText;
	[SerializeField] private TextMeshProUGUI itemCountText;
	[SerializeField] private TextMeshProUGUI modifiersCountText;
	[SerializeField] private TextMeshProUGUI emptyHintText;

	private InventoryModel inventory;

	public void Bind(InventoryModel model)
	{
		Unbind();
		inventory = model;

		if (inventory == null)
		{
			return;
		}

		inventory.OnCashCountChanged += OnCashChanged;
		inventory.DiceAdded += OnDiceChanged;
		inventory.DiceRemoved += OnDiceChanged;
		inventory.ItemsChanged += OnItemsChanged;

		RefreshAll();
	}

	public void Unbind()
	{
		if (inventory == null)
		{
			return;
		}

		inventory.OnCashCountChanged -= OnCashChanged;
		inventory.DiceAdded -= OnDiceChanged;
		inventory.DiceRemoved -= OnDiceChanged;
		inventory.ItemsChanged -= OnItemsChanged;
		inventory = null;
	}

	private void OnDestroy()
	{
		Unbind();
	}

	private void RefreshAll()
	{
		RefreshCash();
		RefreshDice();
		RefreshItems();
		UpdateCounts();
	}

	private void OnCashChanged() => RefreshCash();
	private void OnDiceChanged(string _) => RefreshDice();
	private void OnItemsChanged() => RefreshItems();

	private void RefreshCash()
	{
		if (cashText != null && inventory != null)
		{
			cashText.text = $"Cash: {inventory.CashCount}";
		}
	}

	private void RefreshDice()
	{
		if (inventory == null)
		{
			return;
		}

		PopulateSlots(diceSlots, inventory.DiceIdList);
		UpdateCounts();
		UpdateEmptyHint();
	}

	private void RefreshItems()
	{
		if (inventory == null)
		{
			return;
		}

		var itemNames = new List<string>();
		foreach (var item in inventory.ItemsModel.Items)
		{
			var name = string.IsNullOrWhiteSpace(item.DisplayName) ? item.Id : item.DisplayName;
			if (item.ActivationType == DiceItemActivationType.ClickToActivate)
			{
				name += " (active)";
			}
			itemNames.Add(name);
		}

		PopulateSlots(itemSlots, itemNames);
		UpdateCounts();
		UpdateEmptyHint();
	}

	private void UpdateCounts()
	{
		if (inventory == null)
		{
			return;
		}

		diceCountText?.SetText($"Dice: {inventory.DiceIdList.Count}");
		itemCountText?.SetText($"Items: {inventory.ItemsModel.Items.Count}");
		modifiersCountText?.SetText($"Mods: {inventory.ModifiersModel.AllModifiers.Count}");
	}

	private void UpdateEmptyHint()
	{
		if (emptyHintText == null || inventory == null)
		{
			return;
		}

		bool hasAnything = inventory.DiceIdList.Count > 0 || inventory.ItemsModel.Items.Count > 0;
		emptyHintText.gameObject.SetActive(!hasAnything);
		if (!hasAnything)
		{
			emptyHintText.text = "Inventory is empty. Win dice or find items to fill it.";
		}
	}

	private void PopulateSlots(Transform[] slots, IReadOnlyList<string> labels)
	{
		if (slots == null)
		{
			return;
		}

		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i] == null)
			{
				continue;
			}

			var text = GetOrCreateLabel(slots[i]);

			if (i < labels.Count)
			{
				text.text = labels[i];
				slots[i].gameObject.SetActive(true);
			}
			else
			{
				text.text = string.Empty;
				slots[i].gameObject.SetActive(false);
			}
		}
	}

	private TextMeshProUGUI GetOrCreateLabel(Transform parent)
	{
		var text = parent.GetComponentInChildren<TextMeshProUGUI>();
		if (text != null)
		{
			return text;
		}

		var go = new GameObject("Label");
		go.transform.SetParent(parent, false);
		text = go.AddComponent<TextMeshProUGUI>();
		text.alignment = TextAlignmentOptions.Center;
		text.fontSize = 18;
		return text;
	}
}

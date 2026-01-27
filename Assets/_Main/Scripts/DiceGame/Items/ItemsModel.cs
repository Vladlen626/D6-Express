using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Holds item instances and mirrors them into the modifiers pipeline so they receive stage callbacks.
	/// </summary>
	public class ItemsModel
	{
		private readonly List<IDiceItem> items = new();
		private readonly ModifiersModel modifiersModel;
		private bool defaultsInitialized;

		public ItemsModel(ModifiersModel modifiersModel)
		{
			this.modifiersModel = modifiersModel;
			AddDefaultItems();
		}

		public event Action ItemsChanged;

		public IReadOnlyList<IDiceItem> Items => items;

		/// <summary>
		/// Adds an item to the collection and registers it as a modifier so it participates in all stages.
		/// </summary>
		public void AddItem(IDiceItem item)
		{
			if (item == null)
			{
				return;
			}

			if (items.Exists(existing => string.Equals(existing.Id, item.Id, StringComparison.Ordinal)))
			{
				return;
			}

			items.Add(item);
			modifiersModel?.AddModifier(item);
			ItemsChanged?.Invoke();
		}

		public void RemoveItem(IDiceItem item)
		{
			if (item == null || !items.Remove(item))
			{
				return;
			}

			// Intentionally NOT removing from modifiers pipeline; stages may already be mid-flight.
			ItemsChanged?.Invoke();
		}

		public void Reset()
		{
			foreach (var item in items)
			{
				item.ResetItem();
			}

			items.Clear();
			ItemsChanged?.Invoke();
		}

		private void AddDefaultItems()
		{
			if (defaultsInitialized)
			{
				return;
			}

			defaultsInitialized = true;

			var passMultiplierPrefab = Resources.Load<DiceItemView>("Items/PassMultiplierItem");
			var rerollItemPrefab = Resources.Load<DiceItemView>("Items/RerollSelectedItem");
			var stepUpPrefab = Resources.Load<DiceItemView>("Items/ItemBase");

			// AddItem(new PassMultiplierItem(prefabOverride: passMultiplierPrefab));
			// AddItem(new RerollSelectedItem(prefabOverride: rerollItemPrefab));
			AddItem(new StepUpItem(prefabOverride: stepUpPrefab));
		}
	}
}

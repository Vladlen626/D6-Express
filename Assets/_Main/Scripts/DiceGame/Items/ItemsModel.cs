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
		private readonly DiceGameModel diceGameModel;
		private bool defaultsInitialized;

		public ItemsModel(ModifiersModel modifiersModel, DiceGameModel diceGameModel = null)
		{
			this.modifiersModel = modifiersModel;
			this.diceGameModel = diceGameModel;
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
			(item as IGameModelBoundItem)?.OnAddedToGameModel(diceGameModel);
			ItemsChanged?.Invoke();
		}

		public void RemoveItem(IDiceItem item)
		{
			if (item == null || !items.Remove(item))
			{
				return;
			}

			// Intentionally NOT removing from modifiers pipeline; stages may already be mid-flight.
			(item as IGameModelBoundItem)?.OnRemovedFromGameModel(diceGameModel);
			ItemsChanged?.Invoke();
		}

		public void Reset()
		{
			foreach (var item in items)
			{
				(item as IGameModelBoundItem)?.OnRemovedFromGameModel(diceGameModel);
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
			var silencerPrefab = Resources.Load<DiceItemView>("Items/ItemBase");
			var extraDicePrefab = Resources.Load<DiceItemView>("Items/ItemBase");

			// AddItem(new PassMultiplierItem(prefabOverride: passMultiplierPrefab));
			// AddItem(new RerollSelectedItem(prefabOverride: rerollItemPrefab));
			// AddItem(new StepUpItem(prefabOverride: stepUpPrefab));
			AddItem(new ModifierSilencerItem(prefabOverride: silencerPrefab));
			AddItem(new ExtraDiceCapItem(4, extraDicePrefab));
		}
	}

	/// <summary>
	/// Optional interface for items that need a direct hook into the current DiceGameModel
	/// as soon as they are added/removed (before any modifier stages are fired).
	/// </summary>
	public interface IGameModelBoundItem
	{
		void OnAddedToGameModel(DiceGameModel gameModel);
		void OnRemovedFromGameModel(DiceGameModel gameModel);
	}
}

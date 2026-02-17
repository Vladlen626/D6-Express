using System;
using System.Collections.Generic;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Holds item instances and mirrors them into the modifiers pipeline so they receive stage callbacks.
	/// </summary>
	public class ModifierItemsModel
	{
		private readonly List<IModifierItem> items = new();
		private readonly ModifiersModel modifiersModel;
		private DiceGameModel diceGameModel;

		public ModifierItemsModel(ModifiersModel modifiersModel, DiceGameModel diceGameModel = null)
		{
			this.modifiersModel = modifiersModel;
			this.diceGameModel = diceGameModel;
		}

		public event Action ItemsChanged;

		public IReadOnlyList<IModifierItem> Items => items;

		/// <summary>
		/// Allows late binding of the active DiceGameModel (e.g., when items live in inventory and the game model is created later).
		/// Calls OnAddedToGameModel for already owned items so their effects apply.
		/// </summary>
		public void BindGameModel(DiceGameModel gameModel)
		{
			diceGameModel = gameModel;
			if (diceGameModel == null)
			{
				return;
			}

			foreach (var item in items)
			{
				(item as IGameModelBoundItem)?.OnAddedToGameModel(diceGameModel);
			}
		}

		/// <summary>
		/// Adds an item to the collection and registers it as a modifier so it participates in all stages.
		/// </summary>
		public void AddItem(IModifierItem item)
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

		public void RemoveItem(IModifierItem item)
		{
			if (item == null || !items.Remove(item))
			{
				return;
			}

			modifiersModel?.RemoveModifier(item);
			(item as IGameModelBoundItem)?.OnRemovedFromGameModel(diceGameModel);
			ItemsChanged?.Invoke();
		}

		public void RemoveItemById(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return;
			}

			var item = items.Find(x => string.Equals(x.Id, id, StringComparison.Ordinal));
			if (item != null)
			{
				RemoveItem(item);
			}
		}

		public void Reset()
		{
			foreach (var item in items)
			{
				modifiersModel?.RemoveModifier(item);
				(item as IGameModelBoundItem)?.OnRemovedFromGameModel(diceGameModel);
				item.ResetItem();
			}

			items.Clear();
			ItemsChanged?.Invoke();
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

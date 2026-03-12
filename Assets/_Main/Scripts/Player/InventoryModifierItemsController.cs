using System;
using System.Collections.Generic;
using _Main.Scripts.Dice;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;
using UnityEngine;
using Object = UnityEngine.Object;

public class InventoryModifierItemsController : IBaseController, IActivatable
{
	private const int ConsumedViewTeardownDelayMs = 650;

	private readonly InventoryModel inventory;
	private readonly ModifierItemsModel modifierItemsModel;
	private readonly IObjectFactory objectFactory;
	private readonly InventoryView inventoryView;

	private readonly Dictionary<string, ItemView> viewsByItemId = new(StringComparer.Ordinal);

	public InventoryModifierItemsController(
		InventoryModel inventory,
		InventoryView inventoryView,
		IObjectFactory objectFactory)
	{
		this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
		modifierItemsModel = inventory.ModifierItemsModel ??
			throw new InvalidOperationException("[InventoryModifierItemsController] Inventory.ModifierItemsModel is null.");
		this.inventoryView = inventoryView ? inventoryView :
			throw new ArgumentNullException(nameof(inventoryView));
		this.objectFactory = objectFactory ?? throw new ArgumentNullException(nameof(objectFactory));
	}

	public void Activate()
	{
		inventory.ModifierItemAdded += OnModifierItemAdded;
		inventory.ModifierItemRemoved += OnModifierItemRemoved;
		modifierItemsModel.ItemsChanged += OnItemsChanged;

		RebuildViews();
	}

	public void Deactivate()
	{
		modifierItemsModel.ItemsChanged -= OnItemsChanged;
		inventory.ModifierItemRemoved -= OnModifierItemRemoved;
		inventory.ModifierItemAdded -= OnModifierItemAdded;

		ClearViews();
	}

	private void OnModifierItemAdded(string _)
	{
		RebuildViews();
	}

	private void OnModifierItemRemoved(string _)
	{
		RebuildViews();
	}

	private void OnItemsChanged()
	{
		RebuildViews();
	}

	private void RebuildViews()
	{
		var ownedIds = inventory.ModifierItemIds;
		inventoryView.ValidateModifierItemSlots(ownedIds.Count);

		var slots = inventoryView.ModifierItemSlots;

		for (int i = 0; i < ownedIds.Count; i++)
		{
			var itemId = ownedIds[i];
			var item = ResolveItemOrThrow(itemId);
			var slot = slots[i];

			if (!viewsByItemId.TryGetValue(itemId, out var view) || !view)
			{
				view = SpawnViewOrThrow(item, slot);
				viewsByItemId[itemId] = view;
			}
			else
			{
				BindAndPlace(view, item, slot);
			}
		}

		var staleIds = new List<string>();
		foreach (var pair in viewsByItemId)
		{
			if (!ContainsId(ownedIds, pair.Key))
			{
				staleIds.Add(pair.Key);
			}
		}

		for (int i = 0; i < staleIds.Count; i++)
		{
			var staleId = staleIds[i];
			if (viewsByItemId.TryGetValue(staleId, out var view))
			{
				viewsByItemId.Remove(staleId);
				DestroyViewAsync(view).Forget();
			}
		}
	}

	private IModifierItem ResolveItemOrThrow(string itemId)
	{
		var items = modifierItemsModel.Items;
		for (int i = 0; i < items.Count; i++)
		{
			var item = items[i];
			if (item != null && string.Equals(item.Id, itemId, StringComparison.Ordinal))
			{
				return item;
			}
		}

		throw new InvalidOperationException(
			$"[InventoryModifierItemsController] Item '{itemId}' exists in inventory but has no runtime modifier item instance.");
	}

	private static bool ContainsId(IReadOnlyList<string> ids, string id)
	{
		for (int i = 0; i < ids.Count; i++)
		{
			if (string.Equals(ids[i], id, StringComparison.Ordinal))
			{
				return true;
			}
		}

		return false;
	}

	private static ItemView SpawnViewOrThrow(IModifierItem item, Transform slot)
	{
		if (!slot)
		{
			throw new InvalidOperationException("[InventoryModifierItemsController] Modifier item slot transform is missing.");
		}

		if (item is not IModifierItemViewProvider provider)
		{
			throw new InvalidOperationException(
				$"[InventoryModifierItemsController] Item '{item?.Id}' must implement IModifierItemViewProvider.");
		}

		var prefab = provider.GetViewPrefab();
		if (!prefab)
		{
			throw new InvalidOperationException(
				$"[InventoryModifierItemsController] Item '{item.Id}' returned null ItemView prefab.");
		}

		var view = Object.Instantiate(prefab, slot.position, slot.rotation, slot);
		view.Bind(item);
		return view;
	}

	private static void BindAndPlace(ItemView view, IModifierItem item, Transform slot)
	{
		if (!view)
		{
			throw new InvalidOperationException("[InventoryModifierItemsController] Existing item view reference is missing.");
		}

		if (!slot)
		{
			throw new InvalidOperationException("[InventoryModifierItemsController] Modifier item slot transform is missing.");
		}

		view.Bind(item);
		view.transform.SetParent(slot);
		view.transform.SetPositionAndRotation(slot.position, slot.rotation);
	}

	private async UniTask DestroyViewAsync(ItemView view)
	{
		if (!view)
		{
			return;
		}

		await view.WaitForConsumedAnimationAsync(ConsumedViewTeardownDelayMs);

		if (view)
		{
			objectFactory.Destroy(view.gameObject);
		}
	}

	private void ClearViews()
	{
		foreach (var view in viewsByItemId.Values)
		{
			if (view)
			{
				objectFactory.Destroy(view.gameObject);
			}
		}

		viewsByItemId.Clear();
	}
}

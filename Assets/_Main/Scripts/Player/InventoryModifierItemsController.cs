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
		modifierItemsModel.ItemsChanged += OnItemsChanged;

		RebuildViews();
	}

	public void Deactivate()
	{
		modifierItemsModel.ItemsChanged -= OnItemsChanged;

		ClearViews();
	}

	private void OnItemsChanged()
	{
		RebuildViews();
	}

	private void RebuildViews()
	{
		var runtimeById = BuildRuntimeItemsById();
		var orderedRuntimeItems = BuildOrderedRuntimeItems(runtimeById);
		inventoryView.ValidateModifierItemSlots(orderedRuntimeItems.Count);

		var slots = inventoryView.ModifierItemSlots;
		for (int i = 0; i < orderedRuntimeItems.Count; i++)
		{
			var item = orderedRuntimeItems[i];
			var itemId = item.Id;
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

		var activeIds = new HashSet<string>(StringComparer.Ordinal);
		for (int i = 0; i < orderedRuntimeItems.Count; i++)
		{
			activeIds.Add(orderedRuntimeItems[i].Id);
		}

		var staleIds = new List<string>();
		foreach (var pair in viewsByItemId)
		{
			if (!activeIds.Contains(pair.Key))
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

	private Dictionary<string, IModifierItem> BuildRuntimeItemsById()
	{
		var result = new Dictionary<string, IModifierItem>(StringComparer.Ordinal);
		var runtimeItems = modifierItemsModel.Items;
		for (int i = 0; i < runtimeItems.Count; i++)
		{
			var item = runtimeItems[i];
			if (item == null || string.IsNullOrEmpty(item.Id))
			{
				continue;
			}

			result[item.Id] = item;
		}

		return result;
	}

	private List<IModifierItem> BuildOrderedRuntimeItems(IReadOnlyDictionary<string, IModifierItem> runtimeById)
	{
		var result = new List<IModifierItem>();
		var ownedIds = inventory.ModifierItemIds;

		for (int i = 0; i < ownedIds.Count; i++)
		{
			var id = ownedIds[i];
			if (runtimeById.TryGetValue(id, out var item))
			{
				result.Add(item);
			}
		}

		return result;
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
		view.SetBaseLocalTransform(Vector3.zero, Quaternion.identity, Vector3.one);
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
		view.transform.SetParent(slot, false);
		view.SetBaseLocalTransform(Vector3.zero, Quaternion.identity, Vector3.one);
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

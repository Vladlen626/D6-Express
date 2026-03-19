using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Main.Scripts.Dice
{
	public sealed class DiceGameItemsDisplayManager
	{
		private readonly DiceGameModel diceGameModel;
		private readonly DiceTableView diceTableView;
		private readonly LifecycleService lifecycleService;
		private readonly IObjectFactory objectFactory;
		private readonly GlobalNotificationService notificationService;
		private readonly DiceItemViewRegistry itemViewRegistry;

		private readonly List<IBaseController> itemControllers = new();
		private readonly List<ItemView> itemViews = new();

		public DiceGameItemsDisplayManager(
			DiceGameModel diceGameModel,
			DiceTableView diceTableView,
			LifecycleService lifecycleService,
			IObjectFactory objectFactory,
			DiceItemViewRegistry itemViewRegistry,
			GlobalNotificationService notificationService)
		{
			this.diceGameModel = diceGameModel ?? throw new ArgumentNullException(nameof(diceGameModel));
			this.diceTableView = diceTableView ? diceTableView : throw new ArgumentNullException(nameof(diceTableView));
			this.lifecycleService = lifecycleService ?? throw new ArgumentNullException(nameof(lifecycleService));
			this.objectFactory = objectFactory ?? throw new ArgumentNullException(nameof(objectFactory));
			this.itemViewRegistry = itemViewRegistry ?? throw new ArgumentNullException(nameof(itemViewRegistry));
			this.notificationService = notificationService;
		}

		public async UniTask SetupItemsDisplayAsync()
		{
			var items = diceGameModel.PlayerModifierItemsModel.Items;
			if (items.Count == 0)
			{
				return;
			}

			var slots = diceTableView.ItemSlotsSelection;
			ValidateSlots(slots, items.Count, "selection");

			for (int i = 0; i < items.Count; i++)
			{
				var slot = slots[i];
				var prefab = ResolveItemPrefab(items[i]);
				var view = Object.Instantiate(prefab);
				PlaceViewInSlot(view, slot);

				var controller = new ModifierItemController(items[i], view, diceGameModel, itemViewRegistry, notificationService);
				itemControllers.Add(controller);
				itemViews.Add(view);
				await lifecycleService.RegisterAsync(controller);
			}
		}

		public void MoveItemsToGameSlots()
		{
			if (itemViews.Count == 0)
			{
				return;
			}

			var slots = diceTableView.ItemSlotsGame;
			ValidateSlots(slots, itemViews.Count, "game");

			for (int i = 0; i < itemViews.Count; i++)
			{
				var slot = slots[i];
				var view = itemViews[i];
				PlaceViewInSlot(view, slot);
			}
		}

		public void CleanUpItems()
		{
			lifecycleService.UnregisterControllersGroup(itemControllers);
			itemControllers.Clear();

			foreach (var view in itemViews)
			{
				if (view)
				{
					objectFactory.Destroy(view.gameObject);
				}
			}
			itemViews.Clear();
		}

		private static ItemView ResolveItemPrefab(IModifierItem item)
		{
			if (item is not IModifierItemViewProvider provider)
			{
				throw new InvalidOperationException(
					$"[DiceGame] Item '{item?.Id}' must implement IModifierItemViewProvider to be displayed.");
			}

			var prefab = provider.GetViewPrefab();
			if (!prefab)
			{
				throw new InvalidOperationException(
					$"[DiceGame] Item '{item.Id}' returned null ItemView prefab. Check item catalog visualId/prefab setup.");
			}

			return prefab;
		}

		private static void PlaceViewInSlot(ItemView view, Transform slot)
		{
			view.transform.SetParent(slot, false);
			view.SetBaseLocalTransform(Vector3.zero, Quaternion.identity, Vector3.one);
		}

		private static void ValidateSlots(Transform[] slots, int requiredCount, string groupName)
		{
			if (requiredCount == 0)
			{
				return;
			}

			if (slots == null)
			{
				throw new InvalidOperationException($"[DiceGame] Item slots '{groupName}' are not assigned.");
			}

			if (slots.Length < requiredCount)
			{
				throw new InvalidOperationException(
					$"[DiceGame] Item slots '{groupName}' count ({slots.Length}) is less than required items ({requiredCount}).");
			}

			for (int i = 0; i < requiredCount; i++)
			{
				if (!slots[i])
				{
					throw new InvalidOperationException(
						$"[DiceGame] Item slot '{groupName}' at index {i} is not assigned.");
				}
			}
		}
	}
}

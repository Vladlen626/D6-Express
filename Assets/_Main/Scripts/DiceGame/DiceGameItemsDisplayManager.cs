using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public sealed class DiceGameItemsDisplayManager
	{
		private readonly DiceGameModel diceGameModel;
		private readonly DiceTableView diceTableView;
		private readonly LifecycleService lifecycleService;
		private readonly IObjectFactory objectFactory;

		private readonly List<IBaseController> itemControllers = new();
		private readonly List<DiceItemView> itemViews = new();

		public DiceGameItemsDisplayManager(
			DiceGameModel diceGameModel,
			DiceTableView diceTableView,
			LifecycleService lifecycleService,
			IObjectFactory objectFactory)
		{
			this.diceGameModel = diceGameModel;
			this.diceTableView = diceTableView;
			this.lifecycleService = lifecycleService;
			this.objectFactory = objectFactory;
		}

		public async UniTask SetupItemsDisplayAsync()
		{
			var items = diceGameModel.PlayerModifierItemsModel.Items;
			if (items.Count == 0)
			{
				return;
			}

			if (!diceTableView.ItemViewPrefab)
			{
				Debug.LogWarning("[DiceGame] ItemViewPrefab is not assigned on DiceTableView. Items will not be spawned.");
				return;
			}

			var slots = diceTableView.ItemSlotsSelection;

			for (int i = 0; i < items.Count; i++)
			{
				var slot = slots != null && i < slots.Length ? slots[i] : null;
				var prefab = (items[i] as IModifierItemViewProvider)?.GetViewPrefab() ?? diceTableView.ItemViewPrefab;
				var view = Object.Instantiate(
					prefab,
					slot ? slot.position : Vector3.zero,
					slot ? slot.rotation : Quaternion.identity);

				if (slot)
				{
					view.transform.SetParent(slot);
				}

				var controller = new ModifierItemController(items[i], view);
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
			for (int i = 0; i < itemViews.Count; i++)
			{
				var slot = slots != null && i < slots.Length ? slots[i] : null;
				if (!slot)
				{
					continue;
				}

				var view = itemViews[i];
				view.transform.SetParent(slot);
				view.transform.position = slot.position;
				view.transform.rotation = slot.rotation;
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
	}
}

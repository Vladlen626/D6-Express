using System;
using System.Collections.Generic;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

namespace _Main.Scripts.Dice
{
	public class DiceItemAnalyticsController : IBaseController, IActivatable
	{
		private readonly DiceGameModel diceGameModel;
		private readonly IAnalyticsService analyticsService;
		private readonly HashSet<IModifierItem> subscribedItems = new();

		public DiceItemAnalyticsController(
			DiceGameModel diceGameModel,
			IAnalyticsService analyticsService)
		{
			this.diceGameModel = diceGameModel ?? throw new ArgumentNullException(nameof(diceGameModel));
			this.analyticsService = analyticsService;
		}

		public void Activate()
		{
			if (analyticsService == null)
			{
				return;
			}

			var itemsModel = diceGameModel.PlayerModifierItemsModel;
			if (itemsModel == null)
			{
				return;
			}

			itemsModel.ItemsChanged += OnItemsChanged;
			SyncItemSubscriptions(itemsModel.Items);
		}

		public void Deactivate()
		{
			var itemsModel = diceGameModel.PlayerModifierItemsModel;
			if (itemsModel != null)
			{
				itemsModel.ItemsChanged -= OnItemsChanged;
			}

			ClearItemSubscriptions();
		}

		private void OnItemsChanged()
		{
			var items = diceGameModel.PlayerModifierItemsModel?.Items;
			SyncItemSubscriptions(items);
		}

		private void SyncItemSubscriptions(IReadOnlyList<IModifierItem> items)
		{
			if (items == null)
			{
				ClearItemSubscriptions();
				return;
			}

			var toRemove = new List<IModifierItem>();
			foreach (var item in subscribedItems)
			{
				if (ContainsReference(items, item))
				{
					continue;
				}

				item.ActivationStarted -= OnItemActivationStarted;
				item.EffectApplied -= OnItemEffectApplied;
				toRemove.Add(item);
			}

			for (int i = 0; i < toRemove.Count; i++)
			{
				subscribedItems.Remove(toRemove[i]);
			}

			for (int i = 0; i < items.Count; i++)
			{
				var item = items[i];
				if (item == null || subscribedItems.Contains(item))
				{
					continue;
				}

				item.ActivationStarted += OnItemActivationStarted;
				item.EffectApplied += OnItemEffectApplied;
				subscribedItems.Add(item);
			}
		}

		private void ClearItemSubscriptions()
		{
			foreach (var item in subscribedItems)
			{
				item.ActivationStarted -= OnItemActivationStarted;
				item.EffectApplied -= OnItemEffectApplied;
			}

			subscribedItems.Clear();
		}

		private void OnItemActivationStarted(IModifierItem item)
		{
			if (item == null || analyticsService == null)
			{
				return;
			}

			analyticsService.TrackDiceItemActivation(
				item.Id,
				diceGameModel.DiceGameState,
				item.State,
				diceGameModel.CurrentTurn);
		}

		private void OnItemEffectApplied(IModifierItem item)
		{
			if (item == null || analyticsService == null)
			{
				return;
			}

			analyticsService.TrackDiceItemEffect(
				item.Id,
				diceGameModel.DiceGameState,
				item.State,
				diceGameModel.CurrentTurn);
		}

		private static bool ContainsReference(IReadOnlyList<IModifierItem> items, IModifierItem target)
		{
			if (items == null || target == null)
			{
				return false;
			}

			for (int i = 0; i < items.Count; i++)
			{
				if (ReferenceEquals(items[i], target))
				{
					return true;
				}
			}

			return false;
		}
	}
}

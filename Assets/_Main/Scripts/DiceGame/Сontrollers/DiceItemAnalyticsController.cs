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
		private readonly Dictionary<IModifierItem, bool> subscribedItems = new();
		private ModifierItemsModel playerItemsModel;
		private ModifierItemsModel enemyItemsModel;

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

			playerItemsModel = diceGameModel.PlayerModifierItemsModel;
			enemyItemsModel = diceGameModel.EnemyModifierItemsModel;

			if (playerItemsModel != null)
			{
				playerItemsModel.ItemsChanged += OnPlayerItemsChanged;
				SyncItemSubscriptions(playerItemsModel.Items, true);
			}

			if (enemyItemsModel != null)
			{
				enemyItemsModel.ItemsChanged += OnEnemyItemsChanged;
				SyncItemSubscriptions(enemyItemsModel.Items, false);
			}
		}

		public void Deactivate()
		{
			if (playerItemsModel != null)
			{
				playerItemsModel.ItemsChanged -= OnPlayerItemsChanged;
			}

			if (enemyItemsModel != null)
			{
				enemyItemsModel.ItemsChanged -= OnEnemyItemsChanged;
			}

			ClearItemSubscriptions();
			playerItemsModel = null;
			enemyItemsModel = null;
		}

		private void OnPlayerItemsChanged()
		{
			SyncItemSubscriptions(playerItemsModel?.Items, true);
		}

		private void OnEnemyItemsChanged()
		{
			SyncItemSubscriptions(enemyItemsModel?.Items, false);
		}

		private void SyncItemSubscriptions(IReadOnlyList<IModifierItem> items, bool isPlayerSide)
		{
			if (items == null)
			{
				RemoveItemSubscriptionsBySide(isPlayerSide);
				return;
			}

			var toRemove = new List<IModifierItem>();
			foreach (var pair in subscribedItems)
			{
				if (pair.Value != isPlayerSide || ContainsReference(items, pair.Key))
				{
					continue;
				}

				pair.Key.ActivationStarted -= OnItemActivationStarted;
				pair.Key.EffectApplied -= OnItemEffectApplied;
				toRemove.Add(pair.Key);
			}

			for (int i = 0; i < toRemove.Count; i++)
			{
				subscribedItems.Remove(toRemove[i]);
			}

			for (int i = 0; i < items.Count; i++)
			{
				var item = items[i];
				if (item == null)
				{
					continue;
				}

				if (subscribedItems.TryGetValue(item, out var existingSide))
				{
					if (existingSide == isPlayerSide)
					{
						continue;
					}

					item.ActivationStarted -= OnItemActivationStarted;
					item.EffectApplied -= OnItemEffectApplied;
					subscribedItems.Remove(item);
				}

				item.ActivationStarted += OnItemActivationStarted;
				item.EffectApplied += OnItemEffectApplied;
				subscribedItems[item] = isPlayerSide;
			}
		}

		private void RemoveItemSubscriptionsBySide(bool isPlayerSide)
		{
			var toRemove = new List<IModifierItem>();
			foreach (var pair in subscribedItems)
			{
				if (pair.Value != isPlayerSide)
				{
					continue;
				}

				pair.Key.ActivationStarted -= OnItemActivationStarted;
				pair.Key.EffectApplied -= OnItemEffectApplied;
				toRemove.Add(pair.Key);
			}

			for (int i = 0; i < toRemove.Count; i++)
			{
				subscribedItems.Remove(toRemove[i]);
			}
		}

		private void ClearItemSubscriptions()
		{
			foreach (var item in subscribedItems.Keys)
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

			var isPlayerSide = ResolveItemSide(item);
			analyticsService.TrackDiceItemActivation(
				item.Id,
				diceGameModel.DiceGameState,
				item.State,
				diceGameModel.CurrentTurn,
				isPlayerSide);
		}

		private void OnItemEffectApplied(IModifierItem item)
		{
			if (item == null || analyticsService == null)
			{
				return;
			}

			var isPlayerSide = ResolveItemSide(item);
			analyticsService.TrackDiceItemEffect(
				item.Id,
				diceGameModel.DiceGameState,
				item.State,
				diceGameModel.CurrentTurn,
				isPlayerSide);
		}

		private bool ResolveItemSide(IModifierItem item)
		{
			if (item == null)
			{
				return true;
			}

			if (subscribedItems.TryGetValue(item, out var isPlayerSide))
			{
				return isPlayerSide;
			}

			return ContainsReference(playerItemsModel?.Items, item);
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

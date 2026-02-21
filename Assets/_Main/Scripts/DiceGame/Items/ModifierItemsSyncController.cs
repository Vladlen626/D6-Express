using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;
using PlatformCore.Services.Factory;

namespace _Main.Scripts.Dice
{
	public class ModifierItemsSyncController : IBaseController, IActivatable, IPreloadable
	{
		private readonly InventoryModel inventory;
		private readonly ModifierItemsModel itemsModel;
		private readonly ConfigService configService;
		private readonly DiceScoringService scoringService;
		private IReadOnlyDictionary<string, ItemCatalogEntry> catalog;

		public ModifierItemsSyncController(InventoryModel inventory, ModifierItemsModel itemsModel, ConfigService configService, DiceScoringService scoringService)
		{
			this.inventory = inventory;
			this.itemsModel = itemsModel;
			this.configService = configService;
			this.scoringService = scoringService;
		}

		public async UniTask PreloadAsync()
		{
			catalog = await configService.GetConfigsAsync<ItemCatalogEntry>(ResourcePaths.Json.items_catalog);
		}

		public void Activate()
		{
			inventory.ModifierItemAdded += OnModifierItemAdded;
			inventory.ModifierItemRemoved += OnModifierItemRemoved;

			foreach (var id in inventory.ModifierItemIds)
			{
				AddFromId(id);
			}
		}

		public void Deactivate()
		{
			inventory.ModifierItemAdded -= OnModifierItemAdded;
			inventory.ModifierItemRemoved -= OnModifierItemRemoved;
		}

		private void OnModifierItemAdded(string id)
		{
			AddFromId(id);
		}

		private void OnModifierItemRemoved(string id)
		{
			itemsModel.RemoveItemById(id);
		}

		private void AddFromId(string id)
		{
			if (catalog == null)
			{
				return;
			}

			if (!catalog.TryGetValue(id, out var entry))
			{
				Debug.LogWarning($"[ModifierItemsSync] Catalog entry '{id}' not found.");
				return;
			}

			if (entry.typeEnum != ItemCatalogType.Modifier)
			{
				return;
			}

			var item = ModifierItemFactory.Create(entry, scoringService);
			itemsModel.AddItem(item);
			if (item == null)
			{
				Debug.LogWarning($"[ModifierItemsSync] Failed to create modifier '{id}'.");
			}
		}
	}
}

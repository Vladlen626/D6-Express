using System.Collections.Generic;
using _Main.Scripts.UI;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class TooltipsController : BaseContextController<UITooltip>
	{
		private readonly DiceGameModel diceGameModel;
		private readonly ConfigService configService;
		private readonly DiceTableView tableView;

		private TextsConfig textsConfig;
		private IReadOnlyDictionary<string, ItemCatalogEntry> catalog;

		private DiceModel currentDiceModel;
		private IModifierItem currentItem;
		private readonly List<DiceItemView> itemViews = new();
		private Camera mainCamera;

		public TooltipsController(IUIService uiService, DiceGameModel diceGameModel, ConfigService configService,
			Camera mainCamera, DiceTableView tableView = null)
			: base(uiService)
		{
			this.mainCamera = mainCamera;
			this.diceGameModel = diceGameModel;
			this.configService = configService;
			this.tableView = tableView;
		}

		protected override async UniTask OnPreloadAsync()
		{
			catalog = await configService.GetConfigsAsync<ItemCatalogEntry>(ResourcePaths.Json.items_catalog);
			textsConfig = await configService.GetFirstOrDefaultAsync<TextsConfig>(ResourcePaths.Json.texts_eng);
		}

		protected override void OnActivate()
		{
			base.OnActivate();

			_context.Show();
			_context.HideTooltip();
			diceGameModel.OnDiceGameStateChanged += OnDiceGameStateChangedHandler;
			diceGameModel.ScreenDiceDictChanged += ScreenDiceDictChangedHandler;
			ScreenDiceDictChangedHandler();
			OnDiceGameStateChangedHandler();
		}

		private void OnDiceGameStateChangedHandler()
		{
			_context.Show();
			SubscribeOnItemHoverEvents();
		}

		protected override void OnDeactivate()
		{
			diceGameModel.OnDiceGameStateChanged -= OnDiceGameStateChangedHandler;
			diceGameModel.ScreenDiceDictChanged -= ScreenDiceDictChangedHandler;
			UnsubscribeFromItemHoverEvents();

			base.OnDeactivate();
		}

		private void ScreenDiceDictChangedHandler()
		{
			UnsubscribeFromDiceHoverEvents();
			SubscribeOnDiceHoverEvents();
			SubscribeOnItemHoverEvents();
		}

		private void SubscribeOnDiceHoverEvents()
		{
			foreach (var keyValuePair in diceGameModel.ScreenDiceDict)
			{
				keyValuePair.Value.OnDiceHoverEnter.AddListener(() => OnDiceHoverEnter(keyValuePair.Key));
				keyValuePair.Value.OnDiceHoverExit.AddListener(() => OnDiceHoverExit(keyValuePair.Key));
			}
		}

		private void UnsubscribeFromDiceHoverEvents()
		{
			foreach (var keyValuePair in diceGameModel.ScreenDiceDict)
			{
				keyValuePair.Value.OnDiceHoverEnter.RemoveAllListeners();
				keyValuePair.Value.OnDiceHoverExit.RemoveAllListeners();
			}
		}

		private void SubscribeOnItemHoverEvents()
		{
			UnsubscribeFromItemHoverEvents();

			var views = UnityEngine.Object.FindObjectsByType<DiceItemView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			foreach (var view in views)
			{
				if (!view)
				{
					continue;
				}

				view.OnHoverEnter += OnItemHoverEnter;
				view.OnHoverExit += OnItemHoverExit;
				itemViews.Add(view);
			}
		}

		private void UnsubscribeFromItemHoverEvents()
		{
			foreach (var view in itemViews)
			{
				if (!view)
				{
					continue;
				}

				view.OnHoverEnter -= OnItemHoverEnter;
				view.OnHoverExit -= OnItemHoverExit;
			}

			itemViews.Clear();

			if (currentItem != null)
			{
				currentItem = null;
				if (currentDiceModel != null)
				{
					OnDiceHoverEnter(currentDiceModel);
				}
				else
				{
					_context.HideTooltip();
				}
			}
		}

		private void OnDiceHoverEnter(DiceModel diceModel)
		{
			if (!catalog.TryGetValue(diceModel.ConfigId, out var diceConfig) || diceConfig.typeEnum != ItemCatalogType.Dice)
			{
				return;
			}

			currentDiceModel = diceModel;

			if (currentItem != null)
			{
				return;
			}

			var header = textsConfig.texts[diceConfig.nameKey];
			var description = textsConfig.texts[diceConfig.descriptionKey];
			_context.SetHeaderText(header);
			_context.SetDescriptionText(description);
			_context.SetRarity(diceConfig.rarityEnum);


			var pos = diceModel.CurrentPosition;
			if (diceGameModel.DiceGameState != DiceGameState.GAME && tableView)
			{
				pos = tableView.TooltipPos;
			}

			_context.SetPositionFromWorld(
				pos,
				Vector3.zero,
				mainCamera
			);

			_context.ShowTooltip();
		}

		private void OnDiceHoverExit(DiceModel diceModel)
		{
			if (diceModel == currentDiceModel)
			{
				currentDiceModel = null;
				if (currentItem == null)
				{
					_context.HideTooltip();
				}
			}
		}

		private void OnItemHoverEnter(IModifierItem item)
		{
			if (item == null)
			{
				return;
			}

			if (!catalog.TryGetValue(item.Id, out var entry) || entry.typeEnum != ItemCatalogType.Modifier)
			{
				return;
			}

			currentItem = item;

			var header = textsConfig.texts[entry.nameKey];
			var description = textsConfig.texts[entry.descriptionKey];
			_context.SetHeaderText(header);
			_context.SetDescriptionText(description);
			_context.SetRarity(entry.rarityEnum);

			if (tableView && tableView.TooltipPos)
			{
				_context.SetPositionFromWorld(tableView.TooltipPos, Vector3.zero, mainCamera);
			}

			_context.ShowTooltip();
		}

		private void OnItemHoverExit(IModifierItem item)
		{
			if (item == null || currentItem != item)
			{
				return;
			}

			currentItem = null;

			if (currentDiceModel != null)
			{
				OnDiceHoverEnter(currentDiceModel);
				return;
			}

			_context.HideTooltip();
		}
	}
}

using System.Collections.Generic;
using _Main.Scripts.Core;
using _Main.Scripts.UI;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services;
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
		private readonly ICameraService cameraService;

		private TextsConfig textsConfig;
		private IReadOnlyDictionary<string, ItemCatalogEntry> catalog;

		private DiceModel currentDiceModel;
		private IModifierItem currentItem;
		private ItemView currentItemView;
		private readonly List<ItemView> itemViews = new();
		private Camera mainCamera;

		public TooltipsController(IUIService uiService, DiceGameModel diceGameModel, ConfigService configService,
			ICameraService cameraService, Camera mainCamera, DiceTableView tableView = null)
			: base(uiService)
		{
			this.mainCamera = mainCamera;
			this.diceGameModel = diceGameModel;
			this.configService = configService;
			this.tableView = tableView;
			this.cameraService = cameraService;
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
			_context.SetActivationLabel(null);
			_context.HideTooltip();
			if (cameraService != null)
			{
				cameraService.ActiveCameraChanged += OnActiveCameraChanged;
			}
			diceGameModel.OnDiceGameStateChanged += OnDiceGameStateChangedHandler;
			diceGameModel.OnCurrentTurnChanged += OnCurrentTurnChangedHandler;
			diceGameModel.OnDiceAnimationInProgressChanged += OnDiceAnimationInProgressChangedHandler;
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
			if (cameraService != null)
			{
				cameraService.ActiveCameraChanged -= OnActiveCameraChanged;
			}
			diceGameModel.OnDiceAnimationInProgressChanged -= OnDiceAnimationInProgressChangedHandler;
			diceGameModel.OnCurrentTurnChanged -= OnCurrentTurnChangedHandler;
			diceGameModel.OnDiceGameStateChanged -= OnDiceGameStateChangedHandler;
			diceGameModel.ScreenDiceDictChanged -= ScreenDiceDictChangedHandler;
			UnsubscribeFromItemHoverEvents();
			ClearCurrentHoverAndHideTooltip();

			base.OnDeactivate();
		}

		private void ScreenDiceDictChangedHandler()
		{
			if (currentDiceModel != null && !diceGameModel.ScreenDiceDict.ContainsKey(currentDiceModel))
			{
				ClearCurrentHoverAndHideTooltip();
			}

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

			var views = UnityEngine.Object.FindObjectsByType<ItemView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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
				currentItemView = null;
				if (currentDiceModel != null)
				{
					OnDiceHoverEnter(currentDiceModel);
				}
				else
				{
					_context.SetActivationLabel(null);
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
			_context.SetActivationLabel(null);
			_context.SetRarity(diceConfig.rarityEnum);

			if (tableView)
			{
				_context.SetStaticPosition();
			}
			else
			{
				_context.SetPositionFromWorld(
					diceModel.CurrentPosition,
					Vector3.zero,
					mainCamera
				);
			}

			_context.ShowTooltip();
		}

		private void OnDiceHoverExit(DiceModel diceModel)
		{
			if (diceModel == currentDiceModel)
			{
				currentDiceModel = null;
				if (currentItem == null)
				{
					_context.SetActivationLabel(null);
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

			if (!catalog.TryGetValue(item.Id, out var entry) || entry.typeEnum != ItemCatalogType.ModifierItem)
			{
				return;
			}

			currentItem = item;
			currentItemView = FindItemView(item);

			var header = textsConfig.texts[entry.nameKey];
			var description = textsConfig.texts[entry.descriptionKey];
			_context.SetHeaderText(header);
			_context.SetDescriptionText(description);
			if (TryGetActivationLabel(item, out var activationLabelText, out var activationLabelStyle))
			{
				_context.SetActivationLabel(activationLabelText, activationLabelStyle);
			}
			else
			{
				_context.SetActivationLabel(null);
			}
			_context.SetRarity(entry.rarityEnum);

			SetItemTooltipPosition();

			_context.ShowTooltip();
		}

		private void OnItemHoverExit(IModifierItem item)
		{
			if (item == null || currentItem != item)
			{
				return;
			}

			currentItem = null;
			currentItemView = null;

			if (currentDiceModel != null)
			{
				OnDiceHoverEnter(currentDiceModel);
				return;
			}

			_context.SetActivationLabel(null);
			_context.HideTooltip();
		}

		private bool TryGetActivationLabel(
			IModifierItem item,
			out string activationLabelText,
			out TooltipActivationLabelStyle activationLabelStyle)
		{
			activationLabelText = null;
			activationLabelStyle = TooltipActivationLabelStyle.PreMatch;

			if (item is not IItemTooltipActivationLabelProvider provider)
			{
				return false;
			}

			if (!provider.TooltipActivationLabel.HasValue)
			{
				return false;
			}

			var localizationKey = provider.TooltipActivationLabel.Value switch
			{
				ItemTooltipActivationLabel.PreMatch => GlobalConstants.Localization.ItemTooltipActivationPreMatch,
				ItemTooltipActivationLabel.InMatch => GlobalConstants.Localization.ItemTooltipActivationInMatch,
				_ => null
			};
			if (string.IsNullOrWhiteSpace(localizationKey))
			{
				return false;
			}

			activationLabelText = textsConfig.texts[localizationKey];
			activationLabelStyle = provider.TooltipActivationLabel.Value == ItemTooltipActivationLabel.InMatch
				? TooltipActivationLabelStyle.InMatch
				: TooltipActivationLabelStyle.PreMatch;
			return true;
		}

		private void OnActiveCameraChanged(CameraStateEnum _)
		{
			ClearCurrentHoverAndHideTooltip();
		}

		private void OnCurrentTurnChangedHandler(int oldValue, int newValue)
		{
			ClearCurrentHoverAndHideTooltip();
		}

		private void OnDiceAnimationInProgressChangedHandler(bool oldValue, bool newValue)
		{
			if (newValue)
			{
				ClearCurrentHoverAndHideTooltip();
			}
		}

		private void ClearCurrentHoverAndHideTooltip()
		{
			if (!_context)
			{
				currentDiceModel = null;
				currentItem = null;
				currentItemView = null;
				return;
			}

			currentDiceModel = null;
			currentItem = null;
			currentItemView = null;
			_context.SetActivationLabel(null);
			_context.HideTooltip();
		}

		private void SetItemTooltipPosition()
		{
			if (cameraService != null && cameraService.ActiveCameraState == CameraStateEnum.Inventory)
			{
				if (currentItemView)
				{
					_context.SetPositionFromWorld(currentItemView.transform, Vector3.zero, mainCamera);
				}

				return;
			}

			if (tableView)
			{
				_context.SetStaticPosition();
				return;
			}

			if (currentItemView)
			{
				_context.SetPositionFromWorld(currentItemView.transform, Vector3.zero, mainCamera);
			}
		}

		private ItemView FindItemView(IModifierItem item)
		{
			for (var i = 0; i < itemViews.Count; i++)
			{
				var view = itemViews[i];
				if (!view)
				{
					continue;
				}

				if (object.ReferenceEquals(view.BoundItem, item))
				{
					return view;
				}
			}

			return null;
		}
	}
}

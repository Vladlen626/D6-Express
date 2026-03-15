using System.Collections.Generic;
using _Main.Scripts.Dice;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;

public class ModifierAppliedNotificationController : IBaseController, IActivatable, IPreloadable
{
	private readonly ModifierItemsModel modifierItemsModel;
	private readonly ModifiersModel modifiersModel;
	private readonly GlobalNotificationService notificationService;
	private readonly ConfigService configService;
	private readonly ILocalizationService localizationService;

	private readonly HashSet<IModifierItem> subscribedItems = new();
	private IReadOnlyDictionary<string, ItemCatalogEntry> catalog;
	private IReadOnlyDictionary<string, ModifierUIConfig> modifierUiCatalog;

	public ModifierAppliedNotificationController(
		ModifierItemsModel modifierItemsModel,
		ModifiersModel modifiersModel,
		GlobalNotificationService notificationService,
		ConfigService configService,
		ILocalizationService localizationService)
	{
		this.modifierItemsModel = modifierItemsModel;
		this.modifiersModel = modifiersModel;
		this.notificationService = notificationService;
		this.configService = configService;
		this.localizationService = localizationService;
	}

	public async UniTask PreloadAsync()
	{
		catalog = await configService.GetConfigsAsync<ItemCatalogEntry>(ResourcePaths.Json.items_catalog);
		modifierUiCatalog = await configService.GetConfigsAsync<ModifierUIConfig>(ResourcePaths.Json.modifiers_ui);
	}

	public void Activate()
	{
		if (modifierItemsModel != null)
		{
			modifierItemsModel.ItemsChanged += OnItemsChanged;
			SyncItemSubscriptions();
		}

		if (modifiersModel != null)
		{
			modifiersModel.ModifierApplied += OnGlobalModifierApplied;
		}
	}

	public void Deactivate()
	{
		if (modifierItemsModel != null)
		{
			modifierItemsModel.ItemsChanged -= OnItemsChanged;
			ClearItemSubscriptions();
		}

		if (modifiersModel != null)
		{
			modifiersModel.ModifierApplied -= OnGlobalModifierApplied;
		}
	}

	private void OnItemsChanged()
	{
		SyncItemSubscriptions();
	}

	private void SyncItemSubscriptions()
	{
		var items = modifierItemsModel?.Items;
		if (items == null)
		{
			ClearItemSubscriptions();
			return;
		}

		var toRemove = new List<IModifierItem>();
		foreach (var item in subscribedItems)
		{
			if (!ContainsReference(items, item))
			{
				item.ActivationStarted -= OnItemActivationStarted;
				item.EffectApplied -= OnItemEffectApplied;
				toRemove.Add(item);
			}
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

	private void OnItemEffectApplied(IModifierItem item)
	{
		if (notificationService == null || item == null)
		{
			return;
		}

		var message = ResolveApplyDescription(item);
		if (string.IsNullOrWhiteSpace(message))
		{
			return;
		}

		notificationService.ShowToastRawImmediate(message);
	}

	private void OnItemActivationStarted(IModifierItem item)
	{
		if (notificationService == null || item == null)
		{
			return;
		}

		var message = ResolveArmDescription(item);
		if (string.IsNullOrWhiteSpace(message))
		{
			return;
		}

		notificationService.ShowToastRawImmediate(message);
	}

	private void OnGlobalModifierApplied(IModifier modifier, ModifierStage stage)
	{
		_ = stage;
		if (notificationService == null || modifier == null || modifier is IModifierItem)
		{
			return;
		}

		if (modifier is not IModifierApplyResultProvider applyResultProvider)
		{
			return;
		}

		if (!applyResultProvider.LastApplyHadEffect)
		{
			return;
		}

		var messageKey = ResolveGlobalModifierApplyDescriptionKey(modifier);
		if (string.IsNullOrWhiteSpace(messageKey))
		{
			return;
		}

		notificationService.ShowToastImmediate(messageKey);
	}

	private string ResolveArmDescription(IModifierItem item)
	{
		if (catalog != null &&
		    catalog.TryGetValue(item.Id, out var entry) &&
		    entry.typeEnum == ItemCatalogType.ModifierItem)
		{
			var key = string.IsNullOrWhiteSpace(entry.armDescriptionKey)
				? entry.nameKey
				: entry.armDescriptionKey;

			if (!string.IsNullOrWhiteSpace(key))
			{
				return localizationService != null ? localizationService.GetLocalized(key) : key;
			}
		}

		if (!string.IsNullOrWhiteSpace(item.DisplayName))
		{
			return item.DisplayName;
		}

		return item.Id;
	}

	private string ResolveApplyDescription(IModifierItem item)
	{
		if (catalog != null &&
		    catalog.TryGetValue(item.Id, out var entry) &&
		    entry.typeEnum == ItemCatalogType.ModifierItem)
		{
			var key = string.IsNullOrWhiteSpace(entry.applyDescriptionKey)
				? entry.nameKey
				: entry.applyDescriptionKey;

			if (!string.IsNullOrWhiteSpace(key))
			{
				return localizationService != null ? localizationService.GetLocalized(key) : key;
			}
		}

		if (!string.IsNullOrWhiteSpace(item.DisplayName))
		{
			return item.DisplayName;
		}

		return item.Id;
	}

	private string ResolveGlobalModifierApplyDescriptionKey(IModifier modifier)
	{
		if (modifierUiCatalog == null)
		{
			return null;
		}

		var configKey = ResolveModifierConfigKey(modifier);
		if (!modifierUiCatalog.TryGetValue(configKey, out var config) || config == null)
		{
			return null;
		}

		return config.description;
	}

	private static string ResolveModifierConfigKey(IModifier modifier)
	{
		if (modifier is IModifierUiConfigProvider provider && !string.IsNullOrWhiteSpace(provider.UiConfigId))
		{
			return provider.UiConfigId;
		}

		return modifier.GetType().Name;
	}
}

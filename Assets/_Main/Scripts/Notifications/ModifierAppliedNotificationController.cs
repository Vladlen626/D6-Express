using System.Collections.Generic;
using _Main.Scripts.Dice;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;

public class ModifierAppliedNotificationController : IBaseController, IActivatable, IPreloadable
{
	private readonly ModifiersModel modifiersModel;
	private readonly GlobalNotificationService notificationService;
	private readonly ConfigService configService;
	private readonly ILocalizationService localizationService;

	private IReadOnlyDictionary<string, ItemCatalogEntry> catalog;

	public ModifierAppliedNotificationController(
		ModifiersModel modifiersModel,
		GlobalNotificationService notificationService,
		ConfigService configService,
		ILocalizationService localizationService)
	{
		this.modifiersModel = modifiersModel;
		this.notificationService = notificationService;
		this.configService = configService;
		this.localizationService = localizationService;
	}

	public async UniTask PreloadAsync()
	{
		catalog = await configService.GetConfigsAsync<ItemCatalogEntry>(ResourcePaths.Json.items_catalog);
	}

	public void Activate()
	{
		if (modifiersModel == null)
		{
			return;
		}

		modifiersModel.ModifierApplied += OnModifierApplied;
	}

	public void Deactivate()
	{
		if (modifiersModel == null)
		{
			return;
		}

		modifiersModel.ModifierApplied -= OnModifierApplied;
	}

	private void OnModifierApplied(IModifier modifier, ModifierStage stage)
	{
		if (notificationService == null || modifier == null)
		{
			return;
		}

		var name = ResolveModifierName(modifier);
		if (string.IsNullOrWhiteSpace(name))
		{
			return;
		}

		notificationService.EnqueueToastRaw(name);
	}

	private string ResolveModifierName(IModifier modifier)
	{
		if (modifier is IModifierItem item)
		{
			if (catalog != null && catalog.TryGetValue(item.Id, out var entry) && entry.typeEnum == ItemCatalogType.Modifier)
			{
				return localizationService != null ? localizationService.GetLocalized(entry.nameKey) : entry.nameKey;
			}

			if (!string.IsNullOrWhiteSpace(item.DisplayName))
			{
				return item.DisplayName;
			}

			return item.Id;
		}

		return modifier.GetType().Name;
	}
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;

public class NotificationsController : IBaseController, IActivatable, IPreloadable
{
    private readonly GlobalNotificationService notificationService;
    private readonly InventoryModel inventory;
    private readonly ConfigService configService;
    private readonly ILocalizationService localizationService;

    private IReadOnlyDictionary<string, ItemCatalogEntry> diceConfigsDict;
    private string buyText;

    public NotificationsController(GlobalNotificationService notificationService, InventoryModel inventory, ConfigService configService, ILocalizationService localizationService)
    {
        this.notificationService = notificationService;
        this.inventory = inventory;
        this.configService = configService;
        this.localizationService = localizationService;
    }

    public void Activate()
    {
        inventory.DiceAdded += OnDiceAdded;
    }

    public void Deactivate()
    {
        inventory.DiceAdded -= OnDiceAdded;
    }

    public async UniTask PreloadAsync()
    {
        diceConfigsDict = await configService.GetConfigsAsync<ItemCatalogEntry>(ResourcePaths.Json.items_catalog);
        var textsConfig = await configService.GetFirstOrDefaultAsync<TextsConfig>(ResourcePaths.Json.texts_eng);
        buyText = textsConfig.texts["dice_added_to_inventory_notification"];
    }

    private void OnDiceAdded(string diceId)
    {
        if (!diceConfigsDict.TryGetValue(diceId, out var dice) || dice.typeEnum != ItemCatalogType.Dice)
        {
            return;
        }
        var header = localizationService != null ? localizationService.GetLocalized(dice.nameKey) : dice.nameKey;
        notificationService?.ShowToastRawImmediate(string.Format(buyText, header));
    }
}

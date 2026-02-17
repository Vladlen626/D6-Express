using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;

public class NotificationsController : IBaseController, IActivatable, IPreloadable
{
    private readonly Notifications notifications;
    private readonly InventoryModel inventory;
    private readonly ConfigService configService;

    private IReadOnlyDictionary<string, ItemCatalogEntry> diceConfigsDict;
    private TextsConfig textsConfig;
    private string buyText;

    public NotificationsController(Notifications notifications, InventoryModel inventory, ConfigService configService)
    {
        this.notifications = notifications;
        this.inventory = inventory;
        this.configService = configService;
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
        textsConfig = await configService.GetFirstOrDefaultAsync<TextsConfig>(ResourcePaths.Json.texts_eng);
        buyText = textsConfig.texts["dice_added_to_inventory_notification"];
    }

    private void OnDiceAdded(string diceId)
    {
        if (!diceConfigsDict.TryGetValue(diceId, out var dice) || dice.typeEnum != ItemCatalogType.Dice)
        {
            return;
        }
        var header = textsConfig.texts[dice.nameKey];

        notifications.Add(new Notifications.Notification()
        {
            message = string.Format(buyText, header)
        });
    }
}

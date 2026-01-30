using System.Threading.Tasks;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;
using UnityEngine;

public static class ShopFactory
{
    public static async Task<Shop> GetStationShopAsync(InventoryModel inventoryModel, ConfigService configService)
    {
        var shopsConfig = await configService.GetFirstOrDefaultAsync<ShopsConfig>(ResourcePaths.Json.shop);

        var diceConfigsDict = await configService.GetConfigsAsync<DiceConfig>(ResourcePaths.Json.dice_types);
        return new Shop(inventoryModel, diceConfigsDict, shopsConfig.station);
    }

    public static async Task<Shop> GetTrainShopAsync(InventoryModel inventoryModel, ConfigService configService)
    {
        var shopsConfig = await configService.GetFirstOrDefaultAsync<ShopsConfig>(ResourcePaths.Json.shop);

        var diceConfigsDict = await configService.GetConfigsAsync<DiceConfig>(ResourcePaths.Json.dice_types);
        return new Shop(inventoryModel, diceConfigsDict, shopsConfig.train);
    }

    public static ShopViewController GetShopViewController(Shop shop, ShopView shopView, IObjectFactory objectFactory, Interactor interactor, CharacterView shopkeeper)
    {
        return new ShopViewController(shop, shopView, objectFactory, interactor, shopkeeper);
    }

    public static ShopTooltipsController GetShopTooltipsController(IUIService uiService, Shop shop, Interactor interactor, Camera camera)
    {
        return new ShopTooltipsController(uiService, shop, interactor, camera);
    }
}

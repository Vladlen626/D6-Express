using System.Collections.Generic;
using System.Threading.Tasks;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;
using UnityEngine;

public static class ShopFactory
{
    public static async Task<Shop> GetStationShopAsync(InventoryModel inventoryModel, ConfigService configService, Run run)
    {
        var shopsConfig = await configService.GetFirstOrDefaultAsync<ShopsConfig>(ResourcePaths.Json.shop);
        var catalog = await configService.GetConfigsAsync<ItemCatalogEntry>(ResourcePaths.Json.items_catalog);
        var runConfigs = await configService.GetConfigsAsync<RunConfig>(ResourcePaths.Json.run_rules);
        return new Shop(
            inventoryModel,
            catalog,
            shopsConfig.station,
            () => ResolveRunItemPriceMultiplier(run, runConfigs));
    }

    public static async Task<Shop> GetTrainShopAsync(InventoryModel inventoryModel, ConfigService configService, Run run)
    {
        var shopsConfig = await configService.GetFirstOrDefaultAsync<ShopsConfig>(ResourcePaths.Json.shop);
        var catalog = await configService.GetConfigsAsync<ItemCatalogEntry>(ResourcePaths.Json.items_catalog);
        var runConfigs = await configService.GetConfigsAsync<RunConfig>(ResourcePaths.Json.run_rules);
        return new Shop(
            inventoryModel,
            catalog,
            shopsConfig.train,
            () => ResolveRunItemPriceMultiplier(run, runConfigs));
    }

    public static ShopViewController GetShopViewController(Shop shop, ShopView shopView, IObjectFactory objectFactory, Interactor interactor, CharacterView shopkeeper)
    {
        return new ShopViewController(shop, shopView, objectFactory, interactor, shopkeeper);
    }

    public static ShopTooltipsController GetShopTooltipsController(IUIService uiService, Shop shop, ShopView shopView, Interactor interactor, Camera camera)
    {
        return new ShopTooltipsController(uiService, shop, shopView, interactor, camera);
    }

    private static float ResolveRunItemPriceMultiplier(Run run, Dictionary<string, RunConfig> runConfigs)
    {
        if (runConfigs == null)
        {
            return 1f;
        }

        if (!runConfigs.TryGetValue(run.RunRulesId, out var runConfig) || runConfig == null)
        {
            return 1f;
        }

        return runConfig.shop_item_price_multiplier;
    }
}

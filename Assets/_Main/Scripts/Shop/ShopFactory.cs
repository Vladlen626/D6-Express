using System.Threading.Tasks;
using PlatformCore.Services.Factory;

public static class ShopFactory
{
    public static async Task<ShopViewController> GetShopViewController(SceneContext sceneContext, InventoryModel inventoryModel, RunModel runModel, IObjectFactory objectFactory, ConfigService configService)
    {
        var shopConfig = await configService.GetFirstOrDefaultAsync<ShopConfig>(ResourcePaths.Json.shop);
        var shop = new Shop(runModel, inventoryModel, shopConfig);
        return new ShopViewController(shop, sceneContext.Shop, objectFactory);
    }
}

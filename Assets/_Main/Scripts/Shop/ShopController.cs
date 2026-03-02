using System.Collections.Generic;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

public class ShopController : IBaseController, IActivatable, IGameStateChanger
{
    private readonly Run run;
    private readonly Shop trainShop;
    private readonly Shop stationShop;

    public ShopController(Run run, Shop trainShop, Shop stationShop)
    {
        this.run = run;
        this.trainShop = trainShop;
        this.stationShop = stationShop;
    }

    public void Activate()
    {
        run.TickChanged += ResetShops;
    }

    public void Deactivate()
    {
        run.TickChanged -= ResetShops;
    }

    public IEnumerable<(GameStateTransitionTask task, GameStateChangeFunc func)> GetStateChangeFuncs()
    {
        yield return (GameStateTransitionTask.SHOP_RESTOCK, async (x) => ResetShops());
    }

    private void ResetShops()
    {
        RestockAllShops();
        ResetAllRestockPrices();
    }

    private void RestockAllShops()
    {
        trainShop.Restock();
        stationShop.Restock();
    }

    private void ResetAllRestockPrices()
    {
        trainShop.ResetRestockPrice();
        stationShop.ResetRestockPrice();
    }
}

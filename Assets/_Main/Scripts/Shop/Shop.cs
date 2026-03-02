using System;
using System.Collections.Generic;
using System.Linq;

public class Shop : IGameStateChanger
{
    private const int SLOTS = 3;
    private readonly InventoryModel inventoryModel;
    private readonly IReadOnlyDictionary<string, ItemCatalogEntry> catalog;
    private readonly ShopConfig config;
    private readonly TradeItem[] tradeItems = new TradeItem[SLOTS];

    public IReadOnlyList<TradeItem> TradeItems => tradeItems;
    public int RestockPrice { get; private set; }
    public int RestockPriceScale => config.restock_price_scale;

    public event Action RestockPriceChanged;
    public event Action<int, TradeItem> ItemAdded;
    public event Action<int, TradeItem> ItemRemoved;

    public event Action<TradeItem> BuyCompleted;
    public event Action BuyFailed;

    public event Action RestockFailed;

    public Shop(InventoryModel inventoryModel, IReadOnlyDictionary<string, ItemCatalogEntry> catalog, ShopConfig config)
    {
        this.inventoryModel = inventoryModel;
        this.catalog = catalog;
        this.config = config;

        ResetRestockPrice();
    }

    public IEnumerable<(GameStateTransitionTask task, GameStateChangeFunc func)> GetStateChangeFuncs()
    {
        yield return (GameStateTransitionTask.SHOP_RESTOCK, async (x) => Restock());
    }

    public bool CanRestock()
    {
        return inventoryModel.CashCount >= RestockPrice;
    }

    public bool TryRestockForPrice()
    {
        if (CanRestock())
        {
            inventoryModel.TakeCash(RestockPrice);
            Restock();
            IncreaseRestockPrice();
            return true;
        }
        else
        {
            RestockFailed?.Invoke();
            return false;
        }
    }

    public void ResetRestockPrice()
    {
        RestockPrice = config.restock_price;
        RestockPriceChanged?.Invoke();
    }

    public void Restock()
    {
        for (int i = 0; i < tradeItems.Length; i++)
        {
            TradeItem item = tradeItems[i];

            if (item != null)
            {
                item.Buyed -= OnBuyed;
                ItemRemoved?.Invoke(i, item);
            }
            tradeItems[i] = null;
        }

        var unused = config.items.ToList();

        for (int i = 0; i < SLOTS; i++)
        {
            var itemConfigIndex = UnityEngine.Random.Range(0, unused.Count);
            var itemConfig = unused[itemConfigIndex];
            unused.RemoveAt(itemConfigIndex);

            if (!catalog.TryGetValue(itemConfig.itemId, out var entry))
            {
                continue;
            }

            var visualId = string.IsNullOrEmpty(entry.visualId) ? entry.id : entry.visualId;
            var tradeItem = new TradeItem(entry.id, entry.typeEnum, entry.price, visualId);
            tradeItem.Buyed += OnBuyed;
            tradeItems[i] = tradeItem;

            ItemAdded?.Invoke(i, tradeItem);
        }
    }

    private void OnBuyed(TradeItem tradeItem)
    {
        if (inventoryModel.CashCount >= tradeItem.Price)
        {
            inventoryModel.TakeCash(tradeItem.Price);

            switch (tradeItem.ItemType)
            {
                case ItemCatalogType.Dice:
                    inventoryModel.AddDice(tradeItem.ItemId);
                    break;
                case ItemCatalogType.Modifier:
                    inventoryModel.AddModifierItem(tradeItem.ItemId);
                    break;
            }

            tradeItem.Buyed -= OnBuyed;
            var index = Array.IndexOf(tradeItems, tradeItem);
            tradeItems[index] = null;
            ItemRemoved?.Invoke(index, tradeItem);

            BuyCompleted?.Invoke(tradeItem);
        }
        else
        {
            BuyFailed?.Invoke();
        }
    }

    private void IncreaseRestockPrice()
    {
        RestockPrice += RestockPriceScale;
        RestockPriceChanged?.Invoke();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlatformCore.Core;

public class Shop
{
    private const int SLOTS = 3;

    private readonly InventoryModel inventoryModel;
    private readonly IReadOnlyDictionary<string, DiceConfig> dicesConfigs;
    private readonly ShopConfig config;
    private readonly List<TradeItem> tradeItems = new();

    public IReadOnlyList<TradeItem> TradeItems => tradeItems;

    public event Action<int, TradeItem> ItemAdded;
    public event Action<int, TradeItem> ItemRemoved;
    public event Action BuyFailed;

    public Shop(InventoryModel inventoryModel, IReadOnlyDictionary<string, DiceConfig> dicesConfigs, ShopConfig config)
    {
        this.inventoryModel = inventoryModel;
        this.dicesConfigs = dicesConfigs;
        this.config = config;
    }

    public async Task Restock()
    {
        for (int i = 0; i < tradeItems.Count; i++)
        {
            TradeItem item = tradeItems[i];
            item.Buyed -= OnBuyed;
            ItemRemoved?.Invoke(i, item);
        }
        tradeItems.Clear();

        var unused = config.items.ToList();

        for (int i = 0; i < SLOTS; i++)
        {
            var itemConfigIndex = UnityEngine.Random.Range(0, unused.Count);
            var itemConfig = unused[itemConfigIndex];
            unused.RemoveAt(itemConfigIndex);

            var tradeItem = new TradeItem(itemConfig.itemId, dicesConfigs[itemConfig.itemId].price);
            tradeItem.Buyed += OnBuyed;
            tradeItems.Add(tradeItem);

            ItemAdded?.Invoke(i, tradeItem);
        }
    }

    private void OnBuyed(TradeItem tradeItem)
    {
        if (inventoryModel.CashCount >= tradeItem.Price)
        {
            inventoryModel.TakeCash(tradeItem.Price);

            // todo: добавлять не только дайсы
            inventoryModel.AddDice(tradeItem.ItemId);

            tradeItem.Buyed -= OnBuyed;
            var index = tradeItems.IndexOf(tradeItem);
            tradeItems.RemoveAt(index);
            ItemRemoved?.Invoke(index, tradeItem);
        }
        else
        {
            BuyFailed?.Invoke();
        }
    }
}
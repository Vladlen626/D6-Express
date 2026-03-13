using System;
using System.Collections.Generic;

public class Shop : IGameStateChanger
{
    private const int SLOTS = 3;
    private const int COMMON_WEIGHT = 50;
    private const int UNCOMMON_WEIGHT = 30;
    private const int RARE_WEIGHT = 10;
    private const int LEGENDARY_WEIGHT = 5;
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

        var candidates = BuildCandidates();

        for (int i = 0; i < SLOTS; i++)
        {
            if (candidates.Count == 0)
            {
                break;
            }

            var candidateIndex = PickCandidateIndex(candidates);
            var candidate = candidates[candidateIndex];
            candidates.RemoveAt(candidateIndex);

            var entry = candidate.Entry;
            var visualId = string.IsNullOrEmpty(entry.visualId) ? entry.id : entry.visualId;
            var tradeItem = new TradeItem(entry.id, entry.typeEnum, entry.price, visualId);
            tradeItem.Buyed += OnBuyed;
            tradeItems[i] = tradeItem;

            ItemAdded?.Invoke(i, tradeItem);
        }
    }

    private List<ShopCandidate> BuildCandidates()
    {
        var result = new List<ShopCandidate>();
        if (config.items == null)
        {
            return result;
        }

        foreach (var itemConfig in config.items)
        {
            if (itemConfig == null || string.IsNullOrEmpty(itemConfig.itemId))
            {
                continue;
            }

            if (!catalog.TryGetValue(itemConfig.itemId, out var entry))
            {
                continue;
            }

            if (!IsSupportedShopItemType(entry.typeEnum))
            {
                UnityEngine.Debug.LogError(
                    $"[Shop] Catalog entry '{entry.id}' has unsupported item type '{entry.typeEnum}' and will be skipped.");
                continue;
            }

            // Modifier items are currently unique in inventory; skip already owned ones.
            if (entry.typeEnum == ItemCatalogType.ModifierItem && HasModifierItem(entry.id))
            {
                continue;
            }

            var weight = GetRarityWeight(entry.rarityEnum);
            result.Add(new ShopCandidate(entry, weight));
        }

        return result;
    }

    private static int PickCandidateIndex(List<ShopCandidate> candidates)
    {
        var totalWeight = 0;
        for (int i = 0; i < candidates.Count; i++)
        {
            totalWeight += Math.Max(0, candidates[i].Weight);
        }

        if (totalWeight <= 0)
        {
            return UnityEngine.Random.Range(0, candidates.Count);
        }

        var roll = UnityEngine.Random.Range(0, totalWeight);
        var cumulative = 0;
        for (int i = 0; i < candidates.Count; i++)
        {
            cumulative += Math.Max(0, candidates[i].Weight);
            if (roll < cumulative)
            {
                return i;
            }
        }

        return candidates.Count - 1;
    }

    private static int GetRarityWeight(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.UNCOMMON:
                return UNCOMMON_WEIGHT;
            case Rarity.RARE:
                return RARE_WEIGHT;
            case Rarity.LEGENDARY:
                return LEGENDARY_WEIGHT;
            case Rarity.COMMON:
            default:
                return COMMON_WEIGHT;
        }
    }

    private readonly struct ShopCandidate
    {
        public readonly ItemCatalogEntry Entry;
        public readonly int Weight;

        public ShopCandidate(ItemCatalogEntry entry, int weight)
        {
            Entry = entry;
            Weight = weight;
        }
    }

    private void OnBuyed(TradeItem tradeItem)
    {
        if (inventoryModel.CashCount < tradeItem.Price)
        {
            BuyFailed?.Invoke();
            return;
        }

        switch (tradeItem.ItemType)
        {
            case ItemCatalogType.Dice:
                inventoryModel.TakeCash(tradeItem.Price);
                RemoveTradeItemFromStock(tradeItem);
                inventoryModel.AddDice(tradeItem.ItemId);
                break;
            case ItemCatalogType.ModifierItem:
                if (HasModifierItem(tradeItem.ItemId))
                {
                    BuyFailed?.Invoke();
                    return;
                }

                inventoryModel.TakeCash(tradeItem.Price);
                RemoveTradeItemFromStock(tradeItem);
                inventoryModel.AddModifierItem(tradeItem.ItemId);
                break;
            default:
                UnityEngine.Debug.LogError(
                    $"[Shop] Unsupported purchase type '{tradeItem.ItemType}' for item '{tradeItem.ItemId}'. Purchase aborted.");
                BuyFailed?.Invoke();
                return;
        }

        BuyCompleted?.Invoke(tradeItem);
    }

    private static bool IsSupportedShopItemType(ItemCatalogType itemType)
    {
        switch (itemType)
        {
            case ItemCatalogType.Dice:
            case ItemCatalogType.ModifierItem:
                return true;
            default:
                return false;
        }
    }

    private bool HasModifierItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return false;
        }

        var ownedItems = inventoryModel.ModifierItemIds;
        for (int i = 0; i < ownedItems.Count; i++)
        {
            if (string.Equals(ownedItems[i], itemId, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private void RemoveTradeItemFromStock(TradeItem tradeItem)
    {
        if (tradeItem == null)
        {
            return;
        }

        tradeItem.Buyed -= OnBuyed;

        var index = Array.IndexOf(tradeItems, tradeItem);
        if (index < 0)
        {
            return;
        }

        tradeItems[index] = null;
        ItemRemoved?.Invoke(index, tradeItem);
    }

    private void IncreaseRestockPrice()
    {
        RestockPrice += RestockPriceScale;
        RestockPriceChanged?.Invoke();
    }
}

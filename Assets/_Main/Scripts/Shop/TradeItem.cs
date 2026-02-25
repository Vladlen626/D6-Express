using System;


public class TradeItem
{
    public string ItemId { get; private set; }
    public ItemCatalogType ItemType { get; private set; }
    public int Price { get; private set; }
    public string VisualId { get; private set; }

    public event Action<TradeItem> Buyed;

    public TradeItem(string itemId, ItemCatalogType itemType, int price, string visualId)
    {
        ItemId = itemId;
        ItemType = itemType;
        Price = price;
        VisualId = visualId;
    }

    public void Buy()
    {
        Buyed?.Invoke(this);
    }
}

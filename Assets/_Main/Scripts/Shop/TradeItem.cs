using System;

public class TradeItem
{
    public string ItemId { get; private set; }
    public int Price { get; private set; }
    public string Description { get; private set; }

    public event Action<TradeItem> Buyed;

    public TradeItem(string itemId, int price, string description)
    {
        ItemId = itemId;
        Price = price;
        Description = description;
    }

    public void Buy()
    {
        Buyed?.Invoke(this);
    }
}

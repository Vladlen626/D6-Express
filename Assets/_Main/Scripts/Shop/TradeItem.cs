using System;


public class TradeItem
{
    public string ItemId { get; private set; }
    public int Price { get; private set; }

    public event Action<TradeItem> Buyed;

    public TradeItem(string itemId, int price)
    {
        ItemId = itemId;
        Price = price;
    }

    public void Buy()
    {
        Buyed?.Invoke(this);
    }
}

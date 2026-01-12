
using System.Collections.Generic;
using _Main.Scripts.Dice;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;
using UnityEngine;

public class ShopViewController : IBaseController, IActivatable
{
    private readonly Shop shop;
    private readonly ShopView shopView;
    private readonly IObjectFactory objectFactory;

    private readonly List<TradeItemView> items = new();

    public ShopViewController(Shop shop, ShopView shopView, IObjectFactory objectFactory)
    {
        this.shop = shop;
        this.shopView = shopView;
        this.objectFactory = objectFactory;
    }

    public void Activate()
    {
        shop.ItemAdded += OnTradeItemAdded;
        shop.ItemRemoved += OnTradeItemRemoved;
    }

    public void Deactivate()
    {
        shop.ItemRemoved -= OnTradeItemRemoved;
        shop.ItemAdded -= OnTradeItemAdded;
    }

    private async void OnTradeItemAdded(int index, TradeItem tradeItem)
    {
        var tradeItemView = await objectFactory.CreateAsync<TradeItemView>(ResourcePaths.Shop.TradeItem, shopView.Slots[index].ItemTfm.position, shopView.Slots[index].ItemTfm.rotation, shopView.Slots[index].ItemTfm);
        tradeItemView.Buyed += OnBuyed;

        var view = await objectFactory.CreateAsync<ShopItemDiceView>(ResourcePaths.Shop.ShopItemDice, tradeItemView.transform.position, tradeItemView.transform.rotation, tradeItemView.transform);
        view.Initialize(tradeItem.ItemId);

        shopView.Slots[index].SetPrice(tradeItem.Price.ToString());

        items.Add(tradeItemView);
    }

    private void OnBuyed(TradeItemView tradeItemView)
    {
        var viewIndex = items.IndexOf(tradeItemView);
        shop.TradeItems[viewIndex].Buy();
    }

    private void OnTradeItemRemoved(int index, TradeItem tradeItem)
    {
        var tradeItemView = items[index];
        tradeItemView.Buyed -= OnBuyed;

        shopView.Slots[index].SetPrice("x");

        Object.Destroy(tradeItemView.gameObject);
    }
}

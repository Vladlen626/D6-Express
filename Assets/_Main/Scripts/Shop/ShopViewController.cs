
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
    private readonly Interactor interactor;
    private readonly CharacterView shopkeeper;
    private readonly List<(TradeItem item, TradeItemView view)> items = new();

    public ShopViewController(Shop shop, ShopView shopView, IObjectFactory objectFactory, Interactor interactor, CharacterView shopkeeper)
    {
        this.shop = shop;
        this.shopView = shopView;
        this.objectFactory = objectFactory;
        this.interactor = interactor;
        this.shopkeeper = shopkeeper;
    }

    public void Activate()
    {
        shop.ItemAdded += OnTradeItemAdded;
        shop.ItemRemoved += OnTradeItemRemoved;
        shop.BuyFailed += OnBuyFailed;
    }

    public void Deactivate()
    {
        shop.BuyFailed -= OnBuyFailed;
        shop.ItemRemoved -= OnTradeItemRemoved;
        shop.ItemAdded -= OnTradeItemAdded;
    }

    private async void OnTradeItemAdded(int index, TradeItem tradeItem)
    {
        var tradeItemView = await objectFactory.CreateAsync<TradeItemView>(ResourcePaths.Shop.TradeItem, shopView.Slots[index].ItemTfm.position, shopView.Slots[index].ItemTfm.rotation, shopView.Slots[index].ItemTfm);
        tradeItemView.Init(index);

        tradeItemView.Buyed += OnBuyed;

        var view = await objectFactory.CreateAsync<ShopItemDiceView>(ResourcePaths.Shop.ShopItemDice, tradeItemView.transform.position, tradeItemView.transform.rotation, tradeItemView.transform);
        view.Initialize(tradeItem.ItemId);

        shopView.Slots[index].SetPrice(tradeItem.Price.ToString());

        items.Add((tradeItem, tradeItemView));
    }

    private void OnBuyed(TradeItemView tradeItemView)
    {
        var item = items.Find(x => x.view == tradeItemView).item;
        item.Buy();
    }

    private void OnTradeItemRemoved(int index, TradeItem tradeItem)
    {
        var tradeItemViewIndex = items.FindIndex(x => x.item == tradeItem);
        var tradeItemView = items[tradeItemViewIndex].view;

        tradeItemView.Buyed -= OnBuyed;

        shopView.Slots[tradeItemViewIndex].SetPrice("x");

        items.RemoveAt(tradeItemViewIndex);

        tradeItemView.gameObject.SetActive(false);
        Object.Destroy(tradeItemView.gameObject);
    }

    private void OnBuyFailed()
    {
        var interactable = shopkeeper.GetComponent<InteractableSpeakable>();
        interactable.SetId(69);
        interactor.Interact(interactable);
        interactable.ResetId();
    }
}

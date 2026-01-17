
using System.Collections.Generic;
using System.Threading.Tasks;
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

    private int objectsChanging = 0;

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
        shop.RestockFailed += OnRestockFailed;

        shopView.Lever.RestockRequested += OnRestockRequested;
    }

    public void Deactivate()
    {
        shopView.Lever.RestockRequested -= OnRestockRequested;

        shop.RestockFailed -= OnRestockFailed;
        shop.BuyFailed -= OnBuyFailed;
        shop.ItemRemoved -= OnTradeItemRemoved;
        shop.ItemAdded -= OnTradeItemAdded;
    }

    private async void OnTradeItemAdded(int index, TradeItem tradeItem)
    {
        objectsChanging++;

        foreach (var item in items)
        {
            if (item.view.Index == index)
            {
                await item.view.Hiding();
                break;
            }
        }

        var tradeItemView = await objectFactory.CreateAsync<TradeItemView>(ResourcePaths.Shop.TradeItem, shopView.Slots[index].ItemTfm.position, shopView.Slots[index].ItemTfm.rotation, shopView.Slots[index].ItemTfm);
        tradeItemView.Init(index);
        items.Add((tradeItem, tradeItemView));

        tradeItemView.Buyed += OnBuyed;

        var view = await objectFactory.CreateAsync<ShopItemDiceView>(ResourcePaths.Shop.ShopItemDice, tradeItemView.transform.position, tradeItemView.transform.rotation, tradeItemView.transform);
        view.Initialize(tradeItem.ItemId);

        await tradeItemView.ShowAsync();

        shopView.Slots[index].SetPrice(tradeItem.Price.ToString());

        objectsChanging--;
    }

    private void OnBuyed(TradeItemView tradeItemView)
    {
        var item = items.Find(x => x.view == tradeItemView).item;
        item.Buy();
    }

    private async void OnTradeItemRemoved(int index, TradeItem tradeItem)
    {
        objectsChanging++;

        foreach (var item in items)
        {
            if (item.view.Index == index)
            {
                await item.view.Showing();
                break;
            }
        }

        var tradeItemViewIndex = items.FindIndex(x => x.item == tradeItem);
        var tradeItemView = items[tradeItemViewIndex].view;

        tradeItemView.Buyed -= OnBuyed;

        shopView.Slots[tradeItemViewIndex].SetPrice("x");

        await tradeItemView.HideAsync();

        tradeItemViewIndex = items.FindIndex(x => x.item == tradeItem);
        items.RemoveAt(tradeItemViewIndex);

        tradeItemView.gameObject.SetActive(false);
        Object.Destroy(tradeItemView.gameObject);

        objectsChanging--;
    }

    private void OnRestockFailed()
    {
        var interactable = shopkeeper.GetComponent<InteractableSpeakable>();
        interactable.SetId(69);
        interactor.Interact(interactable);
        interactable.ResetId();
    }

    private async void OnRestockRequested()
    {
        if (objectsChanging != 0 || shopView.Lever.IsPulling())
        {
            return;
        }

        await Task.WhenAll(
            shopView.Lever.Pull(),
            shop.TryRestockForPrice());
    }

    private void OnBuyFailed()
    {
        var interactable = shopkeeper.GetComponent<InteractableSpeakable>();
        interactable.SetId(69);
        interactor.Interact(interactable);
        interactable.ResetId();
    }
}

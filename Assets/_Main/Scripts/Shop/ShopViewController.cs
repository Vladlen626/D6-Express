using System.Collections.Generic;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;


public class ShopViewController : IBaseController, IActivatable
{
    private readonly Shop shop;
    private readonly ShopView shopView;
    private readonly IObjectFactory objectFactory;
    private readonly Interactor interactor;
    private readonly CharacterView shopkeeper;
    private readonly Dictionary<int, (TradeItem item, TradeItemView view)> itemsByIndex = new();

    private readonly Dictionary<int, UniTask> operationsByIndex = new();

    public bool IsOperationInProgress => operationsByIndex.Count > 0;

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

        shopView.RestockLever.RestockRequested += OnRestockRequested;

        shopView.SetRestockPrice(shop.RestockPrice.ToString());
    }

    public void Deactivate()
    {
        shopView.RestockLever.RestockRequested -= OnRestockRequested;

        shop.RestockFailed -= OnRestockFailed;
        shop.BuyFailed -= OnBuyFailed;
        shop.ItemRemoved -= OnTradeItemRemoved;
        shop.ItemAdded -= OnTradeItemAdded;
    }

    private void OnTradeItemAdded(int index, TradeItem tradeItem)
    {
        operationsByIndex[index] = ExecuteForIndex(index, () => AddTradeItem(index, tradeItem));
    }

    private void OnTradeItemRemoved(int index, TradeItem tradeItem)
    {
        operationsByIndex[index] = ExecuteForIndex(index, () => RemoveTradeItem(index, tradeItem));
    }

    private async UniTask ExecuteForIndex(int index, Func<UniTask> operation)
    {
        if (operationsByIndex.TryGetValue(index, out var previousOperation))
        {
            await previousOperation;
        }

        try
        {
            await operation();
        }
        finally
        {
            operationsByIndex.Remove(index);
        }
    }

    private async UniTask AddTradeItem(int index, TradeItem tradeItem)
    {
        if (itemsByIndex.TryGetValue(index, out var existingItem))
        {
            await existingItem.view.WaitForTransition();
        }

        var tradeItemView = await objectFactory.CreateAsync<TradeItemView>(
            ResourcePaths.Shop.TradeItem,
            shopView.Slots[index].ItemTfm.position,
            shopView.Slots[index].ItemTfm.rotation,
            shopView.Slots[index].ItemTfm);

        tradeItemView.Init(index);
        tradeItemView.Buyed += OnBuyed;

        var view = await objectFactory.CreateAsync<ShopItemDiceView>(
            ResourcePaths.Shop.ShopItemDice,
            tradeItemView.transform.position,
            tradeItemView.transform.rotation,
            tradeItemView.transform);
        view.Initialize(tradeItem.ItemId);
        shopView.Slots[index].SetPrice(tradeItem.Price.ToString());

        itemsByIndex[index] = (tradeItem, tradeItemView);
    }

    private void OnBuyed(TradeItemView tradeItemView)
    {
        if (itemsByIndex.TryGetValue(tradeItemView.Index, out var itemData))
        {
            itemData.item.Buy();
        }
    }

    private async UniTask RemoveTradeItem(int index, TradeItem tradeItem)
    {
        if (!itemsByIndex.TryGetValue(index, out var itemData))
            return;

        var tradeItemView = itemData.view;

        if (!tradeItemView.gameObject.activeSelf || !tradeItemView.gameObject.activeInHierarchy)
        {
            UnityEngine.Object.Destroy(tradeItemView.gameObject);
            operationsByIndex.Remove(index);
            return;
        }

        await itemData.view.WaitForTransition();

        tradeItemView.Buyed -= OnBuyed;
        shopView.Slots[index].SetPrice("x");

        await tradeItemView.HideAsync();

        itemsByIndex.Remove(index);

        tradeItemView.gameObject.SetActive(false);
        UnityEngine.Object.Destroy(tradeItemView.gameObject);
    }

    private void OnRestockFailed()
    {
        var interactable = shopkeeper.GetComponent<InteractableSpeakable>();
        interactable.SetId(69);
        interactor.Interact(interactable);
        interactable.ResetId();
    }

    private void OnRestockRequested()
    {
        if (IsOperationInProgress || shopView.RestockLever.IsPulling)
            return;

        Restock().Forget();
    }

    private async UniTask Restock()
    {
        shop.TryRestockForPrice();
        await shopView.RestockLever.Pull();
    }

    private void OnBuyFailed()
    {
        var interactable = shopkeeper.GetComponent<InteractableSpeakable>();
        interactable.SetId(69);
        interactor.Interact(interactable);
        interactable.ResetId();
    }
}

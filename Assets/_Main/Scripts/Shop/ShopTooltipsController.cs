using System;
using System.Collections.Generic;
using _Main.Scripts.Core;
using _Main.Scripts.Dice;
using _Main.Scripts.UI;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;
using UnityEngine;

public class ShopTooltipsController : BaseContextController<UITooltip>
{
    private readonly Shop shop;
    private readonly ShopView shopView;
    private readonly Interactor interactor;
    private readonly Camera camera;

    private TextsConfig textsConfig;
    private IReadOnlyDictionary<string, ItemCatalogEntry> catalog;
    private readonly Dictionary<string, ItemTooltipActivationLabel> tooltipActivationLabelByItemId =
        new(StringComparer.Ordinal);

    public ShopTooltipsController(IUIService uiService, Shop shop, ShopView shopView, Interactor interactor, Camera camera) : base(uiService)
    {
        this.shop = shop;
        this.shopView = shopView;
        this.interactor = interactor;
        this.camera = camera;
    }

    protected override async UniTask OnPreloadAsync()
    {
        var configService = Locator.Resolve<ConfigService>();
        catalog = await configService.GetConfigsAsync<ItemCatalogEntry>(ResourcePaths.Json.items_catalog);
        textsConfig = await configService.GetFirstOrDefaultAsync<TextsConfig>(ResourcePaths.Json.texts_eng);
        BuildActivationLabelCache();
    }

    protected override void OnActivate()
    {
        base.OnActivate();

        _context.Hide();
        _context.SetActivationLabel(null);
        _context.HideTooltip();

        interactor.Noticed += OnNoticed;
        interactor.Missed += OnMissed;
    }

    protected override void OnDeactivate()
    {
        interactor.Missed -= OnMissed;
        interactor.Noticed -= OnNoticed;

        base.OnDeactivate();
    }

    private void OnNoticed(Interactable interactable)
    {
        // todo: не нрав
        if (interactable is InteractableTradeable)
        {
            // todo: не нрав гетать
            var shopItemView = interactable.GetComponent<TradeItemView>();
            if (shopItemView == null)
            {
                return;
            }

            if (!shopView)
            {
                return;
            }

            var slots = shopView.Slots;
            var index = shopItemView.Index;
            if (slots == null || index < 0 || index >= slots.Count)
            {
                Debug.LogWarning($"[ShopTooltips] Invalid slot index {index} for shop view '{shopView.name}'.");
                return;
            }

            var slot = slots[index];
            if (!slot || !slot.ItemTfm)
            {
                Debug.LogWarning($"[ShopTooltips] Missing slot or ItemTfm for index {index} in shop view '{shopView.name}'.");
                return;
            }

            if (!shopItemView.transform.IsChildOf(slot.ItemTfm))
            {
                return;
            }

            var shopItem = shop.TradeItems[shopItemView.Index];

            if (shopItem == null)
            {
                return;
            }

            _context.Show();

            if (!catalog.TryGetValue(shopItem.ItemId, out var entry))
            {
                return;
            }

            var header = textsConfig.texts[entry.nameKey];
            var description = textsConfig.texts[entry.descriptionKey];

            _context.SetHeaderText(header);
            _context.SetDescriptionText(description);
            if (TryGetActivationLabel(shopItem.ItemId, out var activationLabelText, out var activationLabelStyle))
            {
                _context.SetActivationLabel(activationLabelText, activationLabelStyle);
            }
            else
            {
                _context.SetActivationLabel(null);
            }
            _context.SetRarity(entry.rarityEnum);

            _context.SetPositionFromWorld(
                shopItemView.transform,
                Vector3.zero,
                camera
            );

            _context.ShowTooltip();
        }
    }

    private void OnMissed(Interactable interactable)
    {
        _context.Hide();
        _context.SetActivationLabel(null);
        _context.HideTooltip();
    }

    private void BuildActivationLabelCache()
    {
        tooltipActivationLabelByItemId.Clear();

        foreach (var pair in catalog)
        {
            var entry = pair.Value;
            if (entry == null || entry.typeEnum != ItemCatalogType.ModifierItem)
            {
                continue;
            }

            var runtimeItem = ModifierItemFactory.Create(entry, null, null);
            if (runtimeItem is not IItemTooltipActivationLabelProvider provider ||
                !provider.TooltipActivationLabel.HasValue)
            {
                continue;
            }

            tooltipActivationLabelByItemId[entry.id] = provider.TooltipActivationLabel.Value;
        }
    }

    private bool TryGetActivationLabel(
        string itemId,
        out string activationLabelText,
        out TooltipActivationLabelStyle activationLabelStyle)
    {
        activationLabelText = null;
        activationLabelStyle = TooltipActivationLabelStyle.PreMatch;

        if (string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        if (!tooltipActivationLabelByItemId.TryGetValue(itemId, out var activationLabel))
        {
            return false;
        }

        var localizationKey = activationLabel switch
        {
            ItemTooltipActivationLabel.PreMatch => GlobalConstants.Localization.ItemTooltipActivationPreMatch,
            ItemTooltipActivationLabel.InMatch => GlobalConstants.Localization.ItemTooltipActivationInMatch,
            _ => null
        };
        if (string.IsNullOrWhiteSpace(localizationKey))
        {
            return false;
        }

        if (!textsConfig.texts.TryGetValue(localizationKey, out activationLabelText) ||
            string.IsNullOrWhiteSpace(activationLabelText))
        {
            return false;
        }

        activationLabelStyle = activationLabel == ItemTooltipActivationLabel.InMatch
            ? TooltipActivationLabelStyle.InMatch
            : TooltipActivationLabelStyle.PreMatch;
        return true;
    }
}

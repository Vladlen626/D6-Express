using System.Collections.Generic;
using _Main.Scripts.UI;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;
using UnityEngine;

public class ShopTooltipsController : BaseContextController<UITooltip>
{
    private readonly Shop shop;
    private readonly Interactor interactor;
    private readonly Camera camera;

    private TextsConfig textsConfig;
    private IReadOnlyDictionary<string, DiceConfig> diceConfigsDict;

    public ShopTooltipsController(IUIService uiService, Shop shop, Interactor interactor, Camera camera) : base(uiService)
    {
        this.shop = shop;
        this.interactor = interactor;
        this.camera = camera;
    }

    protected override async UniTask OnPreloadAsync()
    {
        var configService = Locator.Resolve<ConfigService>();
        diceConfigsDict = await configService.GetConfigsAsync<DiceConfig>(ResourcePaths.Json.dice_types);
        textsConfig = await configService.GetFirstOrDefaultAsync<TextsConfig>(ResourcePaths.Json.texts_eng);
    }

    protected override void OnActivate()
    {
        base.OnActivate();

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

            var shopItem = shop.TradeItems[shopItemView.Index];

            _context.Show();

            var dice = diceConfigsDict[shopItem.ItemId];
            var header = textsConfig.texts[dice.name];
            var description = textsConfig.texts[dice.description];

            _context.SetHeaderText(header);
            _context.SetDescriptionText(description);

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
        _context.HideTooltip();
    }
}

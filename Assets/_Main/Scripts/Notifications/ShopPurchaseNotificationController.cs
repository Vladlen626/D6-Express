using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;

public class ShopPurchaseNotificationController : IBaseController, IActivatable, IPreloadable
{
	private const string BuyNotificationKey = "shop_item_bought_notification";

	private readonly Shop shop;
	private readonly GlobalNotificationService notificationService;
	private readonly ConfigService configService;
	private readonly ILocalizationService localizationService;
	private readonly IAnalyticsService analyticsService;
	private readonly string shopId;

	private IReadOnlyDictionary<string, ItemCatalogEntry> catalog;
	private string buyText;

	public ShopPurchaseNotificationController(
		Shop shop,
		GlobalNotificationService notificationService,
		ConfigService configService,
		ILocalizationService localizationService,
		IAnalyticsService analyticsService,
		string shopId)
	{
		this.shop = shop;
		this.notificationService = notificationService;
		this.configService = configService;
		this.localizationService = localizationService;
		this.analyticsService = analyticsService;
		this.shopId = shopId;
	}

	public async UniTask PreloadAsync()
	{
		catalog = await configService.GetConfigsAsync<ItemCatalogEntry>(ResourcePaths.Json.items_catalog);
		var textsConfig = await configService.GetFirstOrDefaultAsync<TextsConfig>(ResourcePaths.Json.texts_eng);
		buyText = textsConfig.texts[BuyNotificationKey];
	}

	public void Activate()
	{
		shop.BuyCompleted += OnBuyCompleted;
	}

	public void Deactivate()
	{
		shop.BuyCompleted -= OnBuyCompleted;
	}

	private void OnBuyCompleted(TradeItem tradeItem)
	{
		if (tradeItem == null)
		{
			return;
		}

		analyticsService.TrackShopPurchase(shopId, tradeItem);

		if (string.IsNullOrWhiteSpace(buyText))
		{
			return;
		}

		var itemName = ResolveItemName(tradeItem);
		if (string.IsNullOrWhiteSpace(itemName))
		{
			return;
		}

		notificationService?.ShowToastRawImmediate(string.Format(buyText, itemName));
	}

	private string ResolveItemName(TradeItem tradeItem)
	{
		if (catalog != null && catalog.TryGetValue(tradeItem.ItemId, out var entry))
		{
			if (localizationService != null)
			{
				return localizationService.GetLocalized(entry.nameKey);
			}

			return entry.nameKey;
		}

		return tradeItem.ItemId;
	}
}

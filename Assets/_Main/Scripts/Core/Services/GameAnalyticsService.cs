using GameAnalyticsSDK;
using PlatformCore.Services;
using UnityEngine;

namespace _Main.Scripts.Core.Services
{
	public class GameAnalyticsService : IAnalyticsService, ISyncInitializable
	{
		private const string SessionStartEvent = "session:start";
		private const string SessionPlaytimeSecondsEvent = "session:playtime_seconds";
		private const string SessionPlaytimeTotalSecondsEvent = "session:playtime_total_seconds";
		private const string RunStartEventPrefix = "run:start";
		private const string RunFinishEventPrefix = "run:finish";
		private const string LocationEventPrefix = "location";
		private const string ShopPurchaseEventPrefix = "shop:buy";
		private const string PlayerPrefsPlaytimeKey = "Analytics.TotalPlaytimeSeconds";
		private const string UnknownValue = "unknown";

		private float _sessionStartTime;
		private bool _sessionStarted;
		private bool _disposed;

		public void Initialize()
		{
			_sessionStartTime = Time.realtimeSinceStartup;

			GameAnalytics.Initialize();
			TrackSessionStart();
		}

		public void TrackRunStarted(Run run)
		{
			var stationId = NormalizeEventPart(run.StationId);
			var level = FormatLevel(run.Level);

			GameAnalytics.NewDesignEvent($"{RunStartEventPrefix}:{stationId}:{level}");
		}

		public void TrackRunFinished(Run run, Run.FinishType finishType)
		{
			string result = string.Empty;
			switch (finishType)
			{
				case Run.FinishType.WIN:
					result = "win";
					break;
				case Run.FinishType.LOSE:
					result = "lose";
					break;
				case Run.FinishType.ABORT:
					result = "abort";
					break;
			}
			var stationId = NormalizeEventPart(run.StationId);
			var level = FormatLevel(run.Level);

			GameAnalytics.NewDesignEvent($"{RunFinishEventPrefix}:{result}:{stationId}:{level}");
		}

		public void TrackLocationChanged(Location location)
		{
			var locationValue = NormalizeEventPart(location.ToString());
			GameAnalytics.NewDesignEvent($"{LocationEventPrefix}:{locationValue}");
		}

		public void TrackShopPurchase(string shopId, TradeItem tradeItem)
		{
			var shopValue = NormalizeEventPart(shopId);
			var itemTypeValue = MapShopItemType(tradeItem.ItemType);
			var itemIdValue = NormalizeEventPart(tradeItem.ItemId);

			GameAnalytics.NewDesignEvent(
				$"{ShopPurchaseEventPrefix}:{shopValue}:{itemTypeValue}:{itemIdValue}",
				tradeItem.Price);
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (!_sessionStarted)
			{
				return;
			}

			var sessionSeconds = Mathf.Max(0f, Time.realtimeSinceStartup - _sessionStartTime);
			if (sessionSeconds <= 0f)
			{
				return;
			}

			GameAnalytics.NewDesignEvent(SessionPlaytimeSecondsEvent, sessionSeconds);

			var totalSeconds = PlayerPrefs.GetFloat(PlayerPrefsPlaytimeKey, 0f) + sessionSeconds;
			PlayerPrefs.SetFloat(PlayerPrefsPlaytimeKey, totalSeconds);
			PlayerPrefs.Save();

			GameAnalytics.NewDesignEvent(SessionPlaytimeTotalSecondsEvent, totalSeconds);
		}

		private void TrackSessionStart()
		{
			if (_sessionStarted)
			{
				return;
			}

			_sessionStarted = true;
			GameAnalytics.NewDesignEvent(SessionStartEvent);
		}

		private static string NormalizeEventPart(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return UnknownValue;
			}

			return value.Trim().ToLowerInvariant()
				.Replace(" ", "_")
				.Replace(":", "_")
				.Replace("/", "_");
		}

		private static string MapShopItemType(ItemCatalogType itemType)
		{
			switch (itemType)
			{
				case ItemCatalogType.Dice:
					return "dice";
				case ItemCatalogType.ModifierItem:
					// Keep legacy analytics dimension stable after enum rename.
					return "modifier";
				default:
					return UnknownValue;
			}
		}

		private static string FormatLevel(int level)
		{
			return $"lvl_{Mathf.Max(0, level)}";
		}
	}
}

using PlatformCore.Services;

namespace _Main.Scripts.Core.Services
{
	public interface IAnalyticsService : IService
	{
		void TrackRunStarted(Run run);
		void TrackRunFinished(Run run, bool won);
		void TrackLocationChanged(Location location);
		void TrackShopPurchase(string shopId, TradeItem tradeItem);
	}
}

using _Main.Scripts.Dice;
using PlatformCore.Services;

namespace _Main.Scripts.Core.Services
{
	public interface IAnalyticsService : IService
	{
		void TrackRunStarted(Run run);
		void TrackRunFinished(Run run, Run.FinishType type);
		void TrackLocationChanged(Location location);
		void TrackShopPurchase(string shopId, TradeItem tradeItem);
		void TrackDiceMatchFinished(
			Run run,
			bool isWin,
			DiceMatchResultReason reason,
			DiceMatchStage stage,
			int playerScore,
			int enemyScore,
			int targetScore,
			int betSize,
			int turnIndex,
			string source);
		void TrackDiceUpgradeChance(string comboId, string sourceCombinationId, float chance, bool passed);
		void TrackDiceUpgradeRoll(string comboId, int rolledFace);
		void TrackDiceUpgradeApplied(
			string comboId,
			bool applied,
			int beforeMin,
			int beforeMax,
			int beforeBonus,
			int afterMin,
			int afterMax,
			int afterBonus);
		void TrackDiceItemActivation(
			string itemId,
			DiceGameState gameState,
			DiceItemState itemState,
			int turnIndex,
			bool isPlayerSide);
		void TrackDiceItemEffect(
			string itemId,
			DiceGameState gameState,
			DiceItemState itemState,
			int turnIndex,
			bool isPlayerSide);
	}
}

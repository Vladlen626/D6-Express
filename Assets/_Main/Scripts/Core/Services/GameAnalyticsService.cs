using _Main.Scripts.Dice;
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
		private const string DiceMatchResultEventPrefix = "dice:match_result";
		private const string DiceMatchScoreEventPrefix = "dice:match_score";
		private const string DiceMatchContextEventPrefix = "dice:match_context";
		private const string DiceMatchBetEvent = "dice:match_bet:size";
		private const string DiceUpgradeChanceEventPrefix = "dice:upgrade_chance";
		private const string DiceUpgradeRollEventPrefix = "dice:upgrade_roll";
		private const string DiceUpgradeApplyEventPrefix = "dice:upgrade_apply";
		private const string DiceUpgradeBeforeEventPrefix = "dice:upgrade_before";
		private const string DiceUpgradeAfterEventPrefix = "dice:upgrade_after";
		private const string DiceUpgradeDeltaEventPrefix = "dice:upgrade_delta";
		private const string DiceItemActivationEventPrefix = "dice:item_activation";
		private const string DiceItemEffectEventPrefix = "dice:item_effect";
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

		public void TrackDiceMatchFinished(
			Run run,
			bool isWin,
			DiceMatchResultReason reason,
			DiceMatchStage stage,
			int playerScore,
			int enemyScore,
			int targetScore,
			int betSize,
			int turnIndex)
		{
			var resultValue = isWin ? "win" : "lose";
			var reasonValue = MapDiceMatchResultReason(reason);
			var stageValue = MapDiceMatchStage(stage);
			var scoreDelta = Mathf.Abs(playerScore - enemyScore);

			GameAnalytics.NewDesignEvent(
				$"{DiceMatchResultEventPrefix}:{resultValue}:{reasonValue}:{stageValue}",
				scoreDelta);
			GameAnalytics.NewDesignEvent($"{DiceMatchScoreEventPrefix}:player", Mathf.Max(0, playerScore));
			GameAnalytics.NewDesignEvent($"{DiceMatchScoreEventPrefix}:enemy", Mathf.Max(0, enemyScore));
			GameAnalytics.NewDesignEvent($"{DiceMatchScoreEventPrefix}:target", Mathf.Max(0, targetScore));
			GameAnalytics.NewDesignEvent(DiceMatchBetEvent, Mathf.Max(0, betSize));
			GameAnalytics.NewDesignEvent($"{DiceMatchContextEventPrefix}:turn", Mathf.Max(0, turnIndex));

			if (run == null)
			{
				return;
			}

			GameAnalytics.NewDesignEvent($"{DiceMatchContextEventPrefix}:level", Mathf.Max(1, run.Level + 1));
			GameAnalytics.NewDesignEvent($"{DiceMatchContextEventPrefix}:day", Mathf.Max(1, run.Day + 1));
			GameAnalytics.NewDesignEvent($"{DiceMatchContextEventPrefix}:match", Mathf.Max(1, run.Tick + 1));
			GameAnalytics.NewDesignEvent(
				$"{DiceMatchContextEventPrefix}:station:{NormalizeEventPart(run.StationId)}");
		}

		public void TrackDiceUpgradeChance(string comboId, string sourceCombinationId, float chance, bool passed)
		{
			var comboValue = NormalizeEventPart(comboId);
			var sourceValue = NormalizeEventPart(sourceCombinationId);
			var resultValue = passed ? "pass" : "fail";
			var chancePercent = Mathf.Clamp01(chance) * 100f;

			GameAnalytics.NewDesignEvent(
				$"{DiceUpgradeChanceEventPrefix}:{comboValue}:{sourceValue}:{resultValue}",
				chancePercent);
		}

		public void TrackDiceUpgradeRoll(string comboId, int rolledFace)
		{
			var comboValue = NormalizeEventPart(comboId);
			var faceValue = Mathf.Clamp(rolledFace, 0, 6);
			GameAnalytics.NewDesignEvent($"{DiceUpgradeRollEventPrefix}:{comboValue}:face_{faceValue}");
		}

		public void TrackDiceUpgradeApplied(
			string comboId,
			bool applied,
			int beforeMin,
			int beforeMax,
			int beforeBonus,
			int afterMin,
			int afterMax,
			int afterBonus)
		{
			var comboValue = NormalizeEventPart(comboId);
			var resultValue = applied ? "applied" : "failed";

			GameAnalytics.NewDesignEvent($"{DiceUpgradeApplyEventPrefix}:{comboValue}:{resultValue}");

			SendDiceUpgradeStatEvent(DiceUpgradeBeforeEventPrefix, comboValue, "min", beforeMin);
			SendDiceUpgradeStatEvent(DiceUpgradeBeforeEventPrefix, comboValue, "max", beforeMax);
			SendDiceUpgradeStatEvent(DiceUpgradeBeforeEventPrefix, comboValue, "bonus", beforeBonus);
			SendDiceUpgradeStatEvent(DiceUpgradeAfterEventPrefix, comboValue, "min", afterMin);
			SendDiceUpgradeStatEvent(DiceUpgradeAfterEventPrefix, comboValue, "max", afterMax);
			SendDiceUpgradeStatEvent(DiceUpgradeAfterEventPrefix, comboValue, "bonus", afterBonus);
			SendDiceUpgradeStatEvent(DiceUpgradeDeltaEventPrefix, comboValue, "min", afterMin - beforeMin);
			SendDiceUpgradeStatEvent(DiceUpgradeDeltaEventPrefix, comboValue, "max", afterMax - beforeMax);
			SendDiceUpgradeStatEvent(DiceUpgradeDeltaEventPrefix, comboValue, "bonus", afterBonus - beforeBonus);
		}

		public void TrackDiceItemActivation(
			string itemId,
			DiceGameState gameState,
			DiceItemState itemState,
			int turnIndex)
		{
			var itemValue = NormalizeEventPart(itemId);
			var stateValue = MapDiceItemState(itemState);
			var phaseValue = MapDiceGameState(gameState);
			GameAnalytics.NewDesignEvent(
				$"{DiceItemActivationEventPrefix}:{itemValue}:{phaseValue}:{stateValue}",
				Mathf.Max(0, turnIndex));
		}

		public void TrackDiceItemEffect(
			string itemId,
			DiceGameState gameState,
			DiceItemState itemState,
			int turnIndex)
		{
			var itemValue = NormalizeEventPart(itemId);
			var stateValue = MapDiceItemState(itemState);
			var phaseValue = MapDiceGameState(gameState);
			GameAnalytics.NewDesignEvent(
				$"{DiceItemEffectEventPrefix}:{itemValue}:{phaseValue}:{stateValue}",
				Mathf.Max(0, turnIndex));
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

		private static void SendDiceUpgradeStatEvent(string eventPrefix, string comboValue, string stat, int value)
		{
			var sign = value > 0 ? "pos" : value < 0 ? "neg" : "zero";
			GameAnalytics.NewDesignEvent($"{eventPrefix}:{comboValue}:{stat}:{sign}", Mathf.Abs(value));
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

		private static string MapDiceMatchResultReason(DiceMatchResultReason reason)
		{
			switch (reason)
			{
				case DiceMatchResultReason.PlayerReachedTarget:
					return "player_reached_target";
				case DiceMatchResultReason.EnemyReachedTarget:
					return "enemy_reached_target";
				case DiceMatchResultReason.SetupFailed:
					return "setup_failed";
				case DiceMatchResultReason.EnemyAiValidationFailed:
					return "enemy_ai_validation_failed";
				case DiceMatchResultReason.EnemyAiException:
					return "enemy_ai_exception";
				case DiceMatchResultReason.DebugForced:
					return "debug_forced";
				default:
					return UnknownValue;
			}
		}

		private static string MapDiceMatchStage(DiceMatchStage stage)
		{
			switch (stage)
			{
				case DiceMatchStage.Setup:
					return "setup";
				case DiceMatchStage.SelectDice:
					return "select_dice";
				case DiceMatchStage.Bet:
					return "bet";
				case DiceMatchStage.Roll:
					return "roll";
				case DiceMatchStage.Pass:
					return "pass";
				case DiceMatchStage.RoundEnd:
					return "round_end";
				case DiceMatchStage.EnemyTurn:
					return "enemy_turn";
				default:
					return UnknownValue;
			}
		}

		private static string MapDiceGameState(DiceGameState gameState)
		{
			switch (gameState)
			{
				case DiceGameState.GAME:
					return "game";
				case DiceGameState.BET:
					return "bet";
				case DiceGameState.SELECT_DICE:
					return "select_dice";
				default:
					return "default";
			}
		}

		private static string MapDiceItemState(DiceItemState itemState)
		{
			switch (itemState)
			{
				case DiceItemState.Hidden:
					return "hidden";
				case DiceItemState.Ready:
					return "ready";
				case DiceItemState.Armed:
					return "armed";
				case DiceItemState.Cooldown:
					return "cooldown";
				case DiceItemState.Consumed:
					return "consumed";
				case DiceItemState.Disabled:
					return "disabled";
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

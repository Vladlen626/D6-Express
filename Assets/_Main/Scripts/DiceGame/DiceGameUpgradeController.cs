using System;
using System.Linq;
using _Main.Scripts.Core;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;
using PlatformCore.Services.Audio;
using PlatformCore.Services.Factory;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Main.Scripts.Dice
{
	public class DiceGameUpgradeController : IBaseController, IActivatable
	{
		private const string UpgradeDiceVisualId = "default";
		private const string FallbackMinLabel = "Min";
		private const string FallbackMaxLabel = "Max";
		private const string FallbackBonusLabel = "Bonus";
		private const string FallbackMinLenLabel = "Min len";
		private const string FallbackMaxLenLabel = "Max len";
		private const string FallbackStopHint = "Click to stop";
		private const float DefaultUpgradeDiceScale = 1f;

		private static readonly int[] UpgradeDiceWeights = { 1, 1, 1, 1, 1, 1 };

		private readonly DiceGameModel diceGameModel;
		private readonly Run run;
		private readonly ILoggerService logger;
		private readonly IAsyncAwaiterPool upgradeAwaiter;
		private readonly IObjectFactory objectFactory;
		private readonly IAudioService audioService;
		private readonly GlobalNotificationService notificationService;
		private readonly ILocalizationService localizationService;
		private readonly DiceTableView diceTableView;

		private DiceView upgradeDiceView;

		public event Action<DiceUpgradeVisualData> UpgradeApplied;
		private Transform UpgradeDicePos => diceTableView ? diceTableView.UpgradeDicePos : null;
		private float UpgradeDiceScale => diceTableView ? diceTableView.UpgradeDiceScreenScale : DefaultUpgradeDiceScale;

		public DiceGameUpgradeController(
			DiceGameModel diceGameModel,
			Run run,
			ILoggerService logger,
			IAsyncAwaiterPool upgradeAwaiter,
			IObjectFactory objectFactory,
			IAudioService audioService,
			GlobalNotificationService notificationService,
			ILocalizationService localizationService,
			DiceTableView diceTableView)
		{
			this.diceGameModel = diceGameModel;
			this.run = run;
			this.logger = logger;
			this.upgradeAwaiter = upgradeAwaiter;
			this.objectFactory = objectFactory;
			this.audioService = audioService;
			this.notificationService = notificationService;
			this.localizationService = localizationService;
			this.diceTableView = diceTableView;
		}

		public void Activate()
		{
			diceGameModel.UpgradeRequested += OnUpgradeRequested;
		}

		public void Deactivate()
		{
			diceGameModel.UpgradeRequested -= OnUpgradeRequested;

			if (upgradeDiceView)
			{
				objectFactory?.Destroy(upgradeDiceView.gameObject);
				upgradeDiceView = null;
			}
		}

		private void OnUpgradeRequested(DiceCombinationResult combinationResult)
		{
			RunUpgradeAsync(combinationResult).RegisterAwaiter(upgradeAwaiter).Forget();
		}

		private UniTask RunUpgradeAsync(DiceCombinationResult combinationResult)
		{
			return TryTriggerUpgradeAsync(combinationResult);
		}

		public void HideUpgradeDie()
		{
			if (upgradeDiceView)
			{
				upgradeDiceView.Hide();
			}
		}

		public void HideGameplayDiceForUpgrade()
		{
			if (diceGameModel.DiceGameState != DiceGameState.GAME)
			{
				return;
			}

			diceGameModel.HideAllDiceGameModels();
		}

		public void RestoreGameplayDiceAfterUpgrade()
		{
			if (diceGameModel.DiceGameState != DiceGameState.GAME)
			{
				return;
			}

			if (diceGameModel.tableModel != null && diceGameModel.tableModel.isFirstRoll)
			{
				diceGameModel.HideAllDiceGameModels();
				return;
			}

			diceGameModel.ShowAllDiceGameModels();
		}

		public void StopUpgradeRoll(int rolledFace)
		{
			if (upgradeDiceView)
			{
				upgradeDiceView.StopUpgradeSpin(rolledFace);
			}
		}

		public async UniTask<bool> TryTriggerUpgradeAsync(DiceCombinationResult combinationResult)
		{
			if (combinationResult.Combinations == null || combinationResult.Combinations.Count == 0)
			{
				return false;
			}

			if (!diceGameModel.IsPlayerTurn)
			{
				return false;
			}

			var activeScoringService = diceGameModel.GetCurrentScoringService();

			// 1) Straight first
			var straightConfig = activeScoringService.GetStraightUpgradeConfig();
			if (straightConfig != null && straightConfig.Chance > 0f && combinationResult.Combinations.Any(e => IsStraightCombination(e.Combination)))
			{
				return await HandleStraightUpgrade(straightConfig, activeScoringService);
			}

			// 2) Of-a-kind
			var ofaConfig = activeScoringService.GetComboUpgradeConfig("ofakind");
			if (ofaConfig != null && ofaConfig.Chance > 0f && combinationResult.Combinations.Any(e => e.Combination == DiceCombination.ThreeOfAKind || e.Combination == DiceCombination.FourOfAKind || e.Combination == DiceCombination.FiveOfAKind || e.Combination == DiceCombination.SixOfAKind))
			{
				return await HandleUpgradeForCombo("ofakind", ofaConfig, activeScoringService);
			}

			return false;
		}

		private bool IsStraightCombination(DiceCombination combination)
		{
			return combination == DiceCombination.Straight_1_6
			       || combination == DiceCombination.Straight_1_5
			       || combination == DiceCombination.Straight_2_6
			       || combination == DiceCombination.StraightLength4
			       || combination == DiceCombination.StraightLength5
			       || combination == DiceCombination.StraightLength6;
		}

		private async UniTask<bool> HandleUpgradeForCombo(string comboId, ComboUpgradeConfig upgradeConfig, DiceScoringService activeScoringService)
		{
			if (diceGameModel.tableModel == null)
			{
				return false;
			}

			if (Random.value > upgradeConfig.Chance)
			{
				if (upgradeConfig.Debug)
				{
					logger?.Log($"[Upgrade:{comboId}] Chance failed ({upgradeConfig.Chance:P0}).");
				}
				await ShowUpgradeFailedAsync();
				return false;
			}

			diceGameModel.tableModel.DisableButtons();
			logger?.Log($"[Upgrade:{comboId}] Triggered upgrade opportunity.");

			if (upgradeConfig.Debug)
			{
				var outcomes = activeScoringService.GetComboUpgradeOutcomes(comboId);
				var outcomeTable = outcomes != null
					? string.Join(", ", outcomes.Select(o => $"{o.Face}:dmin{o.DeltaMin}/dmax{o.DeltaMax}/db{o.DeltaScoreBonus}"))
					: "none";
				var weights = UpgradeDiceWeights != null ? string.Join(",", UpgradeDiceWeights) : "null";
				logger?.Log($"[Upgrade:{comboId}] Upgrade die; chance={upgradeConfig.Chance:P0}; weights=[{weights}]; outcomes={outcomeTable}");
			}

			await ShowUpgradeBannerAsync();
			int rolledFace = await RollUpgradeDieAsync();

			var before = activeScoringService.GetComboUpgradeState(comboId);
			var outcome = activeScoringService.ApplyGenericUpgradeOutcome(comboId, rolledFace, logger, null, run);
			var after = activeScoringService.GetComboUpgradeState(comboId);
			if (outcome != null && before != null && after != null)
			{
				var summary = $"Rolled {rolledFace}: Min {before.Min}->{after.Min}, Max {before.Max}->{after.Max}, Bonus {before.ScoreBonus}->{after.ScoreBonus}";
				logger?.Log($"[Upgrade:{comboId}] {summary} via upgrade die");
				var minLabel = BuildMinLabel(comboId);
				var maxLabel = BuildMaxLabel(comboId);
				var bonusLabel = BuildBonusLabel();
				UpgradeApplied?.Invoke(new DiceUpgradeVisualData(
					comboId,
					GetComboTitle(comboId),
					BuildRolledText(rolledFace),
					minLabel,
					maxLabel,
					bonusLabel,
					BuildHintText(),
					BuildStopHintText(),
					rolledFace,
					before.Min,
					before.Max,
					before.ScoreBonus,
					after.Min,
					after.Max,
					after.ScoreBonus,
					BuildRouletteSlots(activeScoringService.GetComboUpgradeOutcomes(comboId))));
				return true;
			}

			HideUpgradeDie();
			await ShowUpgradeFailedAsync();
			return false;
		}

		private async UniTask<bool> HandleStraightUpgrade(StraightUpgradeConfig upgradeConfig, DiceScoringService activeScoringService)
		{
			if (diceGameModel.tableModel == null)
			{
				return false;
			}

			if (Random.value > upgradeConfig.Chance)
			{
				if (upgradeConfig.Debug)
				{
					logger?.Log($"[Upgrade:straight] Chance failed ({upgradeConfig.Chance:P0}).");
				}
				await ShowUpgradeFailedAsync();
				return false;
			}

			diceGameModel.tableModel.DisableButtons();
			logger?.Log("[Upgrade:straight] Triggered upgrade opportunity.");

			if (upgradeConfig.Debug)
			{
				var outcomes = upgradeConfig.Outcomes;
				var outcomeTable = outcomes != null
					? string.Join(", ", outcomes.Select(o => $"{o.Face}:dmin{o.DeltaMinLen}/dmax{o.DeltaMaxLen}/db{o.DeltaScoreBonus}"))
					: "none";
				var weights = UpgradeDiceWeights != null ? string.Join(",", UpgradeDiceWeights) : "null";
				logger?.Log($"[Upgrade:straight] Upgrade die; chance={upgradeConfig.Chance:P0}; weights=[{weights}]; outcomes={outcomeTable}");
			}

			await ShowUpgradeBannerAsync();
			int rolledFace = await RollUpgradeDieAsync();

			var before = activeScoringService.GetStraightState();
			var outcome = activeScoringService.ApplyStraightUpgradeOutcome(rolledFace, logger, null, run);
			var after = activeScoringService.GetStraightState();
			if (outcome != null)
			{
				var summary = $"Rolled {rolledFace}: Min {before.MinLen}->{after.MinLen}, Max {before.MaxLen}->{after.MaxLen}, Bonus {before.ScoreBonus}->{after.ScoreBonus}";
				logger?.Log($"[Upgrade:straight] {summary} via upgrade die");
				var minLabel = BuildMinLabel("straight");
				var maxLabel = BuildMaxLabel("straight");
				var bonusLabel = BuildBonusLabel();
				UpgradeApplied?.Invoke(new DiceUpgradeVisualData(
					"straight",
					GetComboTitle("straight"),
					BuildRolledText(rolledFace),
					minLabel,
					maxLabel,
					bonusLabel,
					BuildHintText(),
					BuildStopHintText(),
					rolledFace,
					before.MinLen,
					before.MaxLen,
					before.ScoreBonus,
					after.MinLen,
					after.MaxLen,
					after.ScoreBonus,
					BuildRouletteSlots(activeScoringService.GetStraightUpgradeOutcomes())));
				return true;
			}

			HideUpgradeDie();
			await ShowUpgradeFailedAsync();
			return false;
		}

		private async UniTask ShowUpgradeBannerAsync()
		{
			if (notificationService == null)
			{
				return;
			}

			await notificationService.ShowBannerAsync(GlobalConstants.Localization.DiceBannerUpgradeTriggered, 0.8f);
		}

		private async UniTask ShowUpgradeFailedAsync()
		{
			if (notificationService == null)
			{
				return;
			}

			await notificationService.ShowBannerAsync(GlobalConstants.Localization.DiceBannerUpgradeFailed, 0.8f);
		}

		private async UniTask<int> RollUpgradeDieAsync()
		{
			var view = await EnsureUpgradeDieAsync();
			if (!view)
			{
				logger?.LogWarning("[Upgrade] Upgrade dice view is missing. Upgrade outcome will not be applied.");
				return 0;
			}

			int rolledFace = DiceGameUtils.GetWeightedRandomValue(UpgradeDiceWeights);
			var upgradeDicePos = UpgradeDicePos;
			if (upgradeDicePos)
			{
				view.transform.position = upgradeDicePos.position;
				view.transform.rotation = upgradeDicePos.rotation;
			}

			view.transform.localScale = Vector3.one;
			view.SetVisualScale(UpgradeDiceScale);
			view.Show();
			view.StartUpgradeSpin();

			return rolledFace;
		}

		private async UniTask<DiceView> EnsureUpgradeDieAsync()
		{
			if (upgradeDiceView)
			{
				return upgradeDiceView;
			}

			if (objectFactory == null)
			{
				logger?.LogWarning("[Upgrade] Object factory is not set.");
				return null;
			}

			var upgradeDicePos = UpgradeDicePos;
			if (!upgradeDicePos)
			{
				logger?.LogWarning("[Upgrade] Upgrade dice position is not set.");
				return null;
			}

			upgradeDiceView = await objectFactory.CreateAsync<DiceView>(
				ResourcePaths.Items.DicePrefab,
				upgradeDicePos.position,
				upgradeDicePos.rotation);

			if (!upgradeDiceView)
			{
				logger?.LogWarning("[Upgrade] Failed to create upgrade dice view.");
				return null;
			}

			upgradeDiceView.Initialize(UpgradeDiceVisualId, false, audioService);
			upgradeDiceView.Hide();
			return upgradeDiceView;
		}

		private string GetComboTitle(string comboId)
		{
			if (string.Equals(comboId, "straight", StringComparison.OrdinalIgnoreCase))
			{
				return GetLocalizedSafe(GlobalConstants.Localization.DiceUpgradeComboStraight, comboId);
			}

			if (string.Equals(comboId, "ofakind", StringComparison.OrdinalIgnoreCase))
			{
				return GetLocalizedSafe(GlobalConstants.Localization.DiceUpgradeComboOfAKind, comboId);
			}

			return comboId;
		}

		private string BuildHintText()
		{
			return GetLocalizedSafe(GlobalConstants.Localization.DiceUpgradeHintContinue, string.Empty);
		}

		private string BuildStopHintText()
		{
			return GetLocalizedSafe(GlobalConstants.Localization.DiceUpgradeHintStop, FallbackStopHint);
		}

		private string BuildRolledText(int rolledFace)
		{
			var template = GetLocalizedSafe(GlobalConstants.Localization.DiceUpgradeRolled, string.Empty);
			if (string.IsNullOrWhiteSpace(template))
			{
				return rolledFace.ToString();
			}

			if (template.Contains("{0}"))
			{
				return string.Format(template, rolledFace);
			}

			return $"{template} {rolledFace}";
		}

		private string BuildMinLabel(string comboId)
		{
			var isStraight = string.Equals(comboId, "straight", StringComparison.OrdinalIgnoreCase);
			var key = isStraight ? GlobalConstants.Localization.DiceUpgradeLabelMinLen : GlobalConstants.Localization.DiceUpgradeLabelMin;
			var fallback = isStraight ? FallbackMinLenLabel : FallbackMinLabel;
			return GetLocalizedSafe(key, fallback);
		}

		private string BuildMaxLabel(string comboId)
		{
			var isStraight = string.Equals(comboId, "straight", StringComparison.OrdinalIgnoreCase);
			var key = isStraight ? GlobalConstants.Localization.DiceUpgradeLabelMaxLen : GlobalConstants.Localization.DiceUpgradeLabelMax;
			var fallback = isStraight ? FallbackMaxLenLabel : FallbackMaxLabel;
			return GetLocalizedSafe(key, fallback);
		}

		private string BuildBonusLabel()
		{
			return GetLocalizedSafe(GlobalConstants.Localization.DiceUpgradeLabelBonus, FallbackBonusLabel);
		}

		private string GetLocalizedSafe(string key, string fallback)
		{
			if (string.IsNullOrWhiteSpace(key) || localizationService == null)
			{
				return fallback;
			}

			try
			{
				var value = localizationService.GetLocalized(key);
				return string.IsNullOrWhiteSpace(value) ? fallback : value;
			}
			catch
			{
				return fallback;
			}
		}

		private DiceUpgradeRouletteSlotData[] BuildRouletteSlots(StraightUpgradeOutcome[] outcomes)
		{
			var slots = new DiceUpgradeRouletteSlotData[6];
			for (int face = 1; face <= 6; face++)
			{
				var deltaBonus = 0;
				if (outcomes != null)
				{
					var outcome = outcomes.FirstOrDefault(o => o.Face == face);
					if (outcome != null)
					{
						deltaBonus = outcome.DeltaScoreBonus;
					}
				}

				slots[face - 1] = new DiceUpgradeRouletteSlotData(face, FormatBonusDelta(deltaBonus));
			}

			return slots;
		}

		private DiceUpgradeRouletteSlotData[] BuildRouletteSlots(ComboUpgradeOutcome[] outcomes)
		{
			var slots = new DiceUpgradeRouletteSlotData[6];
			for (int face = 1; face <= 6; face++)
			{
				var deltaBonus = 0;
				if (outcomes != null)
				{
					var outcome = outcomes.FirstOrDefault(o => o.Face == face);
					if (outcome != null)
					{
						deltaBonus = outcome.DeltaScoreBonus;
					}
				}

				slots[face - 1] = new DiceUpgradeRouletteSlotData(face, FormatBonusDelta(deltaBonus));
			}

			return slots;
		}

		private string FormatBonusDelta(int deltaBonus)
		{
			if (deltaBonus > 0)
			{
				return $"+{deltaBonus}";
			}

			return deltaBonus.ToString();
		}
	}
}

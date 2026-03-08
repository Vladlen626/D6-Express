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
			TryTriggerUpgradeAsync(combinationResult).RegisterAwaiter(upgradeAwaiter).Forget();
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

		public async UniTask StopUpgradeRollAsync(int rolledFace)
		{
			if (!upgradeDiceView)
			{
				return;
			}

			await upgradeDiceView.StopUpgradeSpinAsync(rolledFace);
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
			var applied = outcome != null;
			var after = activeScoringService.GetComboUpgradeState(comboId);
			if (applied && before != null && after != null)
			{
				Debug.Log($"[UpgradeDebug:ApplyOutcome] combo={comboId}, rolledFace={rolledFace}, outcomeFace={outcome.Face}");
				var summary = $"Rolled {rolledFace}: Min {before.Min}->{after.Min}, Max {before.Max}->{after.Max}, Bonus {before.ScoreBonus}->{after.ScoreBonus}";
				logger?.Log($"[Upgrade:{comboId}] {summary} via upgrade die");
				var title = localizationService.GetLocalized(GlobalConstants.Localization.DiceUpgradeComboOfAKind);
				var minLabel = localizationService.GetLocalized(GlobalConstants.Localization.DiceUpgradeLabelMinLen);
				var maxLabel = localizationService.GetLocalized(GlobalConstants.Localization.DiceUpgradeLabelMaxLen);
				var bonusLabel = localizationService.GetLocalized(GlobalConstants.Localization.DiceUpgradeLabelBonus);
				var hintText = localizationService.GetLocalized(GlobalConstants.Localization.DiceUpgradeHintContinue);
				var stopHintText = localizationService.GetLocalized(GlobalConstants.Localization.DiceUpgradeHintStop);
				UpgradeApplied?.Invoke(new DiceUpgradeVisualData(
					comboId,
					title,
					minLabel,
					maxLabel,
					bonusLabel,
					hintText,
					stopHintText,
					outcome.Face,
					before.Min,
					before.Max,
					before.ScoreBonus,
					after.Min,
					after.Max,
					after.ScoreBonus,
					BuildRouletteSlots(activeScoringService.GetComboUpgradeOutcomes(comboId), minLabel, maxLabel, bonusLabel)));
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
			var applied = outcome != null;
			var after = activeScoringService.GetStraightState();
			if (applied)
			{
				Debug.Log($"[UpgradeDebug:ApplyOutcome] combo=straight, rolledFace={rolledFace}, outcomeFace={outcome.Face}");
				var summary = $"Rolled {rolledFace}: Min {before.MinLen}->{after.MinLen}, Max {before.MaxLen}->{after.MaxLen}, Bonus {before.ScoreBonus}->{after.ScoreBonus}";
				logger?.Log($"[Upgrade:straight] {summary} via upgrade die");
				var title = localizationService.GetLocalized(GlobalConstants.Localization.DiceUpgradeComboStraight);
				var minLabel = localizationService.GetLocalized(GlobalConstants.Localization.DiceUpgradeLabelMinLen);
				var maxLabel = localizationService.GetLocalized(GlobalConstants.Localization.DiceUpgradeLabelMaxLen);
				var bonusLabel = localizationService.GetLocalized(GlobalConstants.Localization.DiceUpgradeLabelBonus);
				var hintText = localizationService.GetLocalized(GlobalConstants.Localization.DiceUpgradeHintContinue);
				var stopHintText = localizationService.GetLocalized(GlobalConstants.Localization.DiceUpgradeHintStop);
				UpgradeApplied?.Invoke(new DiceUpgradeVisualData(
					"straight",
					title,
					minLabel,
					maxLabel,
					bonusLabel,
					hintText,
					stopHintText,
					outcome.Face,
					before.MinLen,
					before.MaxLen,
					before.ScoreBonus,
					after.MinLen,
					after.MaxLen,
					after.ScoreBonus,
					BuildRouletteSlots(activeScoringService.GetStraightUpgradeOutcomes(), minLabel, maxLabel, bonusLabel)));
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
			var view = await GetOrCreateUpgradeDieAsync();
			if (!view)
			{
				logger?.LogWarning("[Upgrade] Upgrade dice view is missing. Upgrade outcome will not be applied.");
				return 0;
			}

			int rolledFace = DiceGameUtils.GetWeightedRandomValue(UpgradeDiceWeights);
			var upgradeDicePos = UpgradeDicePos;
			if (upgradeDicePos)
			{
				view.transform.SetParent(upgradeDicePos, false);
				view.transform.localPosition = Vector3.zero;
				view.transform.localRotation = Quaternion.identity;
			}

			view.transform.localScale = Vector3.one;
			view.Show();
			view.StartUpgradeSpin();

			return rolledFace;
		}

		private async UniTask<DiceView> GetOrCreateUpgradeDieAsync()
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

		private DiceUpgradeRouletteSlotData[] BuildRouletteSlots(
			StraightUpgradeOutcome[] outcomes,
			string minLabel,
			string maxLabel,
			string bonusLabel)
		{
			return BuildRouletteSlots(
				outcomes,
				o => o.Face,
				o => o.DeltaMinLen,
				o => o.DeltaMaxLen,
				o => o.DeltaScoreBonus,
				minLabel,
				maxLabel,
				bonusLabel);
		}

		private DiceUpgradeRouletteSlotData[] BuildRouletteSlots(
			ComboUpgradeOutcome[] outcomes,
			string minLabel,
			string maxLabel,
			string bonusLabel)
		{
			return BuildRouletteSlots(
				outcomes,
				o => o.Face,
				o => o.DeltaMin,
				o => o.DeltaMax,
				o => o.DeltaScoreBonus,
				minLabel,
				maxLabel,
				bonusLabel);
		}

		private static DiceUpgradeRouletteSlotData[] BuildRouletteSlots<TOutcome>(
			TOutcome[] outcomes,
			Func<TOutcome, int> getFace,
			Func<TOutcome, int> getDeltaMin,
			Func<TOutcome, int> getDeltaMax,
			Func<TOutcome, int> getDeltaBonus,
			string minLabel,
			string maxLabel,
			string bonusLabel)
			where TOutcome : class
		{
			var byFace = new TOutcome[7];
			if (outcomes != null)
			{
				for (int i = 0; i < outcomes.Length; i++)
				{
					var outcome = outcomes[i];
					if (outcome == null)
					{
						continue;
					}

					var face = getFace(outcome);
					if (face >= 1 && face <= 6)
					{
						byFace[face] = outcome;
					}
				}
			}

			var slots = new DiceUpgradeRouletteSlotData[6];
			for (int face = 1; face <= 6; face++)
			{
				var outcome = byFace[face];
				var deltaMin = outcome != null ? getDeltaMin(outcome) : 0;
				var deltaMax = outcome != null ? getDeltaMax(outcome) : 0;
				var deltaBonus = outcome != null ? getDeltaBonus(outcome) : 0;
				DetermineAffectedDelta(deltaMin, deltaMax, deltaBonus, out var affectedStat, out var deltaValue);

				var label = affectedStat switch
				{
					DiceUpgradeAffectedStat.Min => minLabel,
					DiceUpgradeAffectedStat.Max => maxLabel,
					_ => bonusLabel
				};

				slots[face - 1] = new DiceUpgradeRouletteSlotData(face, affectedStat, deltaValue, label);
			}

			return slots;
		}

		private static void DetermineAffectedDelta(
			int deltaMin,
			int deltaMax,
			int deltaBonus,
			out DiceUpgradeAffectedStat affectedStat,
			out int deltaValue)
		{
			if (deltaBonus != 0)
			{
				affectedStat = DiceUpgradeAffectedStat.Bonus;
				deltaValue = deltaBonus;
				return;
			}

			if (deltaMin != 0)
			{
				affectedStat = DiceUpgradeAffectedStat.Min;
				deltaValue = deltaMin;
				return;
			}

			if (deltaMax != 0)
			{
				affectedStat = DiceUpgradeAffectedStat.Max;
				deltaValue = deltaMax;
				return;
			}

			affectedStat = DiceUpgradeAffectedStat.Bonus;
			deltaValue = 0;
		}
	}
}

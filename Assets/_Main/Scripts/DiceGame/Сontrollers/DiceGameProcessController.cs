using System;
using System.Collections.Generic;
using System.Linq;
using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;
using PlatformCore.Services.Audio;
using UnityEngine;
using System.Text;
using TMPro;
using UnityEngine.UI;

namespace _Main.Scripts.Dice
{
	public class DiceGameProcessController : IBaseController, IActivatable
	{
		private readonly ILoggerService logger;
		private readonly IAudioService audioService;
		private readonly ICameraShakeService cameraShakeService;
		private readonly DiceGameModel diceGameModel;
		private readonly Run run;
		private readonly GlobalNotificationService notificationService;
		private TableModel tableModel => diceGameModel.tableModel;

		public bool IsProcessing { get; private set; }

		public DiceGameProcessController(
			ILoggerService logger,
			DiceGameModel diceGameModel,
			ICameraShakeService cameraShakeService,
			IAudioService audioService,
			Run run,
			GlobalNotificationService notificationService)
		{
			this.logger = logger;
			this.diceGameModel = diceGameModel;
			this.cameraShakeService = cameraShakeService;
			this.audioService = audioService;
			this.run = run;
			this.notificationService = notificationService;
		}

		public void Activate()
		{
			logger?.Log("[DiceGameController] Activating...");

			diceGameModel.OnRollClicked += HandleRoll;
			diceGameModel.OnPassClicked += HandlePass;
			diceGameModel.DiceValuesChanged += OnDiceValuesChanged;

			foreach (var diceModel in diceGameModel.CurrentDiceModelList)
			{
				diceModel.OnDiceChosenChanged += UpdateUI;
			}

			UpdateUI();
		}

		public void Deactivate()
		{
			logger?.Log("[DiceGameController] Deactivating...");

			diceGameModel.OnRollClicked -= HandleRoll;
			diceGameModel.OnPassClicked -= HandlePass;
			diceGameModel.DiceValuesChanged -= OnDiceValuesChanged;

			foreach (var diceModel in diceGameModel.CurrentDiceModelList)
			{
				diceModel.OnDiceChosenChanged -= UpdateUI;
			}
		}

		// === ОБРАБОТЧИКИ КНОПОК ===

		public void HandleRoll()
		{
			if (IsProcessing)
			{
				return;
			}

			_ = HandleRollAsync();
		}

		// ReSharper disable Unity.PerformanceAnalysis
		public async UniTask HandleRollAsync()
		{
			IsProcessing = true;

			try
			{
				logger?.Log("[DiceGameController] Handle roll");

				diceGameModel.tableModel.DisableButtons();

				if (tableModel.isFirstRoll)
				{
					if (notificationService != null)
					{
						await notificationService.ShowBannerAsync("dice_banner_round_start", 0.8f);
					}
					var roundStartContext = new DiceModifierContext(
						new DiceCombinationResult { Combinations = new List<DiceCombinationEntry>() },
						diceGameModel.GetUnbanked(),
						tableModel,
						diceGameModel,
						ModifierStage.RoundStart,
						run);
					await diceGameModel.GetCurrentModifiersModel().PlayRoundStartActions(roundStartContext);

					tableModel.isFirstRoll = false;
					diceGameModel.ShowAllDiceGameModels();
				}
				

				var activeScoringService = diceGameModel.GetCurrentScoringService();
				bool isHotDice = await TrySaveSelected(
					diceGameModel.GetSelected(),
					activeScoringService.Evaluate(GetValues(diceGameModel.GetSelected())));
				tableModel.SetPreviewPoints(0);

				// Если все кубы забанкированы после сохранения, сбросить пул
				if (isHotDice)
				{
					if (notificationService != null)
					{
						await notificationService.ShowBannerAsync("dice_banner_hot_dice", 1.1f);
					}
					await ResetAllDiceToActiveAsync();
					diceGameModel.ResetAllDices();
				}

				// Роллим актуальные кубы
				var tasks = new List<UniTask>();
				var diceToRoll = diceGameModel.GetUnbanked();
				foreach (var dice in diceToRoll)
				{
					dice.Roll();
					var view = diceGameModel.ScreenDiceDict[dice];
					tasks.Add(view.PlayRollAnimationAsync());
				}

				await UniTask.WhenAll(tasks);
				audioService.PlaySound(SoundNames.DiceDrop);

				await UniTask.Delay(GlobalParameters.Delay / 2);
				
				var diceCombinationResult = activeScoringService.Evaluate(GetValues(diceToRoll));
				var rollModifierContext = new DiceModifierContext(
					diceCombinationResult,
					diceToRoll,
					tableModel,
					diceGameModel,
					ModifierStage.Roll,
					run);

				await diceGameModel.GetCurrentModifiersModel().PlayRollActions(rollModifierContext);

				if (diceCombinationResult.Combinations.Count == 0)
				{
					await HandleFailedRollAsync(diceCombinationResult, diceToRoll);
				}
			}
			finally
			{
				diceGameModel.RollEnded();
				IsProcessing = false;
			}
		}

		private int[] GetValues(DiceModel[] dice)
		{
			var values = new int[dice.Length];
			for (int i = 0; i < dice.Length; i++) values[i] = dice[i].CurrentValue;
			return values;
		}

		private void HandlePass()
		{
			if (IsProcessing)
			{
				return;
			}

			_ = HandlePassAsync();
		}

		public async UniTask HandlePassForCurrentTurnAsync()
		{
			if (IsProcessing)
			{
				return;
			}

			await HandlePassAsync();
		}

		private async UniTask HandlePassAsync()
		{
			IsProcessing = true;

			try
			{
				diceGameModel.tableModel.DisableButtons();
				
				var selected = diceGameModel.GetSelected();
				var activeScoringService = diceGameModel.GetCurrentScoringService();
				var combo = activeScoringService.Evaluate(GetValues(selected));
				var passModifierContext = new DiceModifierContext(
					combo,
					selected,
					tableModel,
					diceGameModel,
					ModifierStage.Pass,
					run);
				await diceGameModel.GetCurrentModifiersModel().PlayPassActions(passModifierContext);
				await TrySaveSelected(selected, passModifierContext.CombinationResult);
				var roundEndContext = new DiceModifierContext(
					passModifierContext.CombinationResult,
					selected,
					tableModel,
					diceGameModel,
					ModifierStage.RoundEnd,
					run);
				await diceGameModel.GetCurrentModifiersModel().PlayRoundEndActions(roundEndContext);
				EndTurn(true);
			}
			finally
			{
				diceGameModel.PassEnded();
				IsProcessing = false;
			}
		}

		private void OnDiceValuesChanged()
		{
			if (IsProcessing || tableModel.isFirstRoll || !diceGameModel.IsPlayerTurn)
			{
				return;
			}

			_ = ValidateCurrentRollAsync();
		}

		private async UniTask ValidateCurrentRollAsync()
		{
			var diceToCheck = diceGameModel.GetUnbanked();
			if (diceToCheck.Length == 0)
			{
				return;
			}

			var activeScoringService = diceGameModel.GetCurrentScoringService();
			var diceCombinationResult = activeScoringService.Evaluate(GetValues(diceToCheck));
			if (diceCombinationResult.Combinations.Count == 0)
			{
				await HandleFailedRollAsync(diceCombinationResult, diceToCheck);
			}
		}

		private async UniTask HandleFailedRollAsync(DiceCombinationResult diceCombinationResult, DiceModel[] diceToRoll)
		{
			audioService.PlaySound(SoundNames.Fail);
			if (notificationService != null)
			{
				await notificationService.ShowBannerAsync("dice_banner_failed", 1.1f);
			}
			await UniTask.Delay(GlobalParameters.Delay);
			var roundEndContext = new DiceModifierContext(
				diceCombinationResult,
				diceToRoll,
				tableModel,
				diceGameModel,
				ModifierStage.RoundEnd,
				run);
			await diceGameModel.GetCurrentModifiersModel().PlayRoundEndActions(roundEndContext);
			EndTurn(false);
		}

		// ReSharper disable Unity.PerformanceAnalysis
		public void EndTurn(bool success)
		{
			diceGameModel.EndTurn(success);
			audioService.PlaySound(SoundNames.TurnChange);
			UpdateUI();
		}

		public async UniTask<bool> TrySaveSelected(DiceModel[] selected, DiceCombinationResult combinationResult)
		{
			var activeScoringService = diceGameModel.GetCurrentScoringService();
			int points = activeScoringService.CalculateTotalScore(combinationResult);
			if (points <= 0)
			{
				return false;
			}

			tableModel.AddTurnPoints(points);
			var tweenList = new List<Tween>();
			foreach (var diceModel in selected)
			{
				diceModel.SetSaved(true);
				diceModel.SetChosen(false);

				var position = tableModel.GetFreeBankedPosition();
				diceModel.SetCurrentPosition(position);
				var view = diceGameModel.ScreenDiceDict[diceModel];
				view.ResetYRotation();
				tweenList.Add(view.MoveToPosition(position.position));
			}

			await UniTaskUtils.WaitAllTweens(tweenList.ToArray());

			await TryTriggerUpgradeIfNeeded(combinationResult);

			return diceGameModel.AllBanked();
		}

		private async UniTask ResetAllDiceToActiveAsync()
		{
			tableModel.ResetAllPositions();
			var tweens = new List<Tween>();
			
			foreach (var diceModel in diceGameModel.CurrentDiceModelList)
			{
				var pos = tableModel.GetFreeActivePosition();
				if (!pos)
				{
					logger?.LogWarning("[DiceGameController] No free active positions while resetting dice.");
					continue;
				}

				diceModel.SetSaved(false);
				diceModel.SetCurrentPosition(pos);

				if (diceGameModel.ScreenDiceDict.TryGetValue(diceModel, out var view) && view)
				{
					tweens.Add(view.MoveToPosition(pos.position));
				}
				else
				{
					logger?.LogWarning($"[DiceGameController] Missing dice view for model {diceModel?.ConfigId} while resetting.");
				}
			}

			await UniTaskUtils.WaitAllTweens(tweens.ToArray());
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

		private DiceModel PickUpgradeDie()
		{
			var pool = diceGameModel.CurrentDiceModelList;
			if (pool == null || pool.Count == 0)
			{
				return null;
			}

			int index = UnityEngine.Random.Range(0, pool.Count);
			return pool[index];
		}

		private async UniTask TryTriggerUpgradeIfNeeded(DiceCombinationResult combinationResult)
		{
			if (combinationResult.Combinations == null || combinationResult.Combinations.Count == 0)
			{
				return;
			}

			if (!diceGameModel.IsPlayerTurn && !diceGameModel.EnemyComboUpgradesEnabled)
			{
				return;
			}

			var activeScoringService = diceGameModel.GetCurrentScoringService();

			// 1) Straight first
			var straightConfig = activeScoringService.GetStraightUpgradeConfig();
			if (straightConfig != null && straightConfig.Chance > 0f && combinationResult.Combinations.Any(e => IsStraightCombination(e.Combination)))
			{
				await HandleStraightUpgrade(straightConfig, activeScoringService);
				return;
			}

			// 2) Of-a-kind
			var ofaConfig = activeScoringService.GetComboUpgradeConfig("ofakind");
			if (ofaConfig != null && ofaConfig.Chance > 0f && combinationResult.Combinations.Any(e => e.Combination == DiceCombination.ThreeOfAKind || e.Combination == DiceCombination.FourOfAKind || e.Combination == DiceCombination.FiveOfAKind || e.Combination == DiceCombination.SixOfAKind))
			{
				await HandleUpgradeForCombo("ofakind", ofaConfig, activeScoringService);
				return;
			}
		}

		private async UniTask HandleUpgradeForCombo(string comboId, ComboUpgradeConfig upgradeConfig, DiceScoringService activeScoringService)
		{
			if (UnityEngine.Random.value > upgradeConfig.Chance)
			{
				if (upgradeConfig.Debug)
				{
					logger?.Log($"[Upgrade:{comboId}] Chance failed ({upgradeConfig.Chance:P0}).");
				}
				return;
			}

			var die = PickUpgradeDie();
			if (die == null)
			{
				logger?.LogWarning($"[Upgrade:{comboId}] No dice available for upgrade roll.");
				return;
			}

			diceGameModel.tableModel.DisableButtons();
			var announcePos = die.CurrentPosition != null
				? die.CurrentPosition.position
				: diceGameModel.tableModel?.GetFreeActivePosition()?.position ?? Vector3.zero;
			logger?.Log($"[Upgrade:{comboId}] Triggered upgrade opportunity.");

			if (upgradeConfig.Debug)
			{
				var weights = die.Weights != null ? string.Join(",", die.Weights) : "null";
				var outcomes = activeScoringService.GetComboUpgradeOutcomes(comboId);
				var outcomeTable = outcomes != null
					? string.Join(", ", outcomes.Select(o => $"{o.Face}:Δmin{o.DeltaMin}/Δmax{o.DeltaMax}/Δb{o.DeltaScoreBonus}"))
					: "none";
				logger?.Log($"[Upgrade:{comboId}] Die {die.ConfigId}; chance={upgradeConfig.Chance:P0}; weights=[{weights}]; outcomes={outcomeTable}");
			}

			var infoPos = die.CurrentPosition != null ? die.CurrentPosition.position : announcePos;

			int rolledFace;
			if (diceGameModel.ScreenDiceDict.TryGetValue(die, out var view))
			{
				view.Show();
				await view.PlayRollAnimationAsync(1.2f);
				rolledFace = DiceGameUtils.GetWeightedRandomValue(die.Weights);
				die.SetValue(rolledFace);
				view.SetRotation(rolledFace);
			}
			else
			{
				rolledFace = DiceGameUtils.GetWeightedRandomValue(die.Weights);
			}

			if (comboId == "straight")
			{
				var before = activeScoringService.GetStraightState();
				var outcome = activeScoringService.ApplyStraightUpgradeOutcome(rolledFace, logger, null, run);
				var after = activeScoringService.GetStraightState();
				if (outcome != null)
				{
					var summary = $"Rolled {rolledFace}: Min {before.MinLen}->{after.MinLen}, Max {before.MaxLen}->{after.MaxLen}, Bonus {before.ScoreBonus}->{after.ScoreBonus}";
					logger?.Log($"[Upgrade:{comboId}] {summary} via die {die.ConfigId}");
				}
			}
			else
			{
				var before = activeScoringService.GetComboUpgradeState(comboId);
				var outcome = activeScoringService.ApplyGenericUpgradeOutcome(comboId, rolledFace, logger, null, run);
				var after = activeScoringService.GetComboUpgradeState(comboId);
				if (outcome != null && before != null && after != null)
				{
					var summary = $"Rolled {rolledFace}: Min {before.Min}->{after.Min}, Max {before.Max}->{after.Max}, Bonus {before.ScoreBonus}->{after.ScoreBonus}";
					logger?.Log($"[Upgrade:{comboId}] {summary} via die {die.ConfigId}");
				}
			}

			UpdateUI();
		}

		private ComboUpgradeOutcome[] ConvertStraightOutcomes(StraightUpgradeOutcome[] src)
		{
			if (src == null) return Array.Empty<ComboUpgradeOutcome>();
			return src.Select(o => new ComboUpgradeOutcome
			{
				Face = o.Face,
				DeltaMin = o.DeltaMinLen,
				DeltaMax = o.DeltaMaxLen,
				DeltaScoreBonus = o.DeltaScoreBonus
			}).ToArray();
		}

		private async UniTask HandleStraightUpgrade(StraightUpgradeConfig upgradeConfig, DiceScoringService activeScoringService)
		{
			if (UnityEngine.Random.value > upgradeConfig.Chance)
			{
				if (upgradeConfig.Debug)
				{
					logger?.Log($"[Upgrade:straight] Chance failed ({upgradeConfig.Chance:P0}).");
				}
				return;
			}

			var die = PickUpgradeDie();
			if (die == null)
			{
				logger?.LogWarning("[Upgrade:straight] No dice available for upgrade roll.");
				return;
			}

			diceGameModel.tableModel.DisableButtons();
			var announcePos = die.CurrentPosition != null
				? die.CurrentPosition.position
				: diceGameModel.tableModel?.GetFreeActivePosition()?.position ?? Vector3.zero;
			logger?.Log("[Upgrade:straight] Triggered upgrade opportunity.");

			if (upgradeConfig.Debug)
			{
				var weights = die.Weights != null ? string.Join(",", die.Weights) : "null";
				var outcomes = upgradeConfig.Outcomes;
				var outcomeTable = outcomes != null
					? string.Join(", ", outcomes.Select(o => $"{o.Face}:Δmin{o.DeltaMinLen}/Δmax{o.DeltaMaxLen}/Δb{o.DeltaScoreBonus}"))
					: "none";
				logger?.Log($"[Upgrade:straight] Die {die.ConfigId}; chance={upgradeConfig.Chance:P0}; weights=[{weights}]; outcomes={outcomeTable}");
			}

			var infoPos = die.CurrentPosition != null ? die.CurrentPosition.position : announcePos;

			int rolledFace;
			if (diceGameModel.ScreenDiceDict.TryGetValue(die, out var view))
			{
				view.Show();
				await view.PlayRollAnimationAsync(1.2f);
				rolledFace = DiceGameUtils.GetWeightedRandomValue(die.Weights);
				die.SetValue(rolledFace);
				view.SetRotation(rolledFace);
			}
			else
			{
				rolledFace = DiceGameUtils.GetWeightedRandomValue(die.Weights);
			}

			var before = activeScoringService.GetStraightState();
			var outcome = activeScoringService.ApplyStraightUpgradeOutcome(rolledFace, logger, null, run);
			var after = activeScoringService.GetStraightState();
			if (outcome != null)
			{
				var summary = $"Rolled {rolledFace}: Min {before.MinLen}->{after.MinLen}, Max {before.MaxLen}->{after.MaxLen}, Bonus {before.ScoreBonus}->{after.ScoreBonus}";
				logger?.Log($"[Upgrade:straight] {summary} via die {die.ConfigId}");
			}

			UpdateUI();
		}

		private void UpdateUI()
		{
			if (tableModel.isFirstRoll)
			{
				diceGameModel.HideAllDiceGameModels();
			}
			else
			{
				diceGameModel.ShowAllDiceGameModels();
			}

			var selectedDice = diceGameModel.GetSelected();
			var selectedValues = new int[selectedDice.Length];
			for (int i = 0; i < selectedDice.Length; i++)
			{
				selectedValues[i] = selectedDice[i].CurrentValue;
			}

			var activeScoringService = diceGameModel.GetCurrentScoringService();

			if (activeScoringService.HasTrash(selectedValues))
			{
				tableModel.SetPreviewPoints(0);
			}
			else
			{
				var combo = activeScoringService.Evaluate(selectedValues);
				tableModel.SetPreviewPoints(activeScoringService.CalculateTotalScore(combo));
			}


			diceGameModel.tableModel.SendUpdateUI();
		}
	}
}

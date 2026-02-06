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

namespace _Main.Scripts.Dice
{
	public class DiceGameProcessController : IBaseController, IActivatable
	{
		private readonly ILoggerService logger;
		private readonly IAudioService audioService;
		private readonly ICameraShakeService cameraShakeService;
		private readonly DiceGameModel diceGameModel;
		private readonly Run run;
		private readonly Notifications notifications;
		private readonly DiceScoringService scoringService = DiceScoringService.Instance;
		private TableModel tableModel => diceGameModel.tableModel;

		public bool IsProcessing { get; private set; }

		public DiceGameProcessController(
			ILoggerService logger,
			DiceGameModel diceGameModel,
			ICameraShakeService cameraShakeService,
			IAudioService audioService,
			Run run,
			Notifications notifications)
		{
			this.logger = logger;
			this.diceGameModel = diceGameModel;
			this.cameraShakeService = cameraShakeService;
			this.audioService = audioService;
			this.run = run;
			this.notifications = notifications;
		}

		public void Activate()
		{
			logger?.Log("[DiceGameController] Activating...");

			diceGameModel.OnRollClicked += HandleRoll;
			diceGameModel.OnPassClicked += HandlePass;

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
					var roundStartContext = new DiceModifierContext(
						new DiceCombinationResult { Combinations = new List<DiceCombinationEntry>() },
						diceGameModel.GetUnbanked(),
						tableModel,
						diceGameModel,
						ModifierStage.RoundStart,
						run);
					await diceGameModel.ModifiersModel.PlayRoundStartActions(roundStartContext);

					tableModel.isFirstRoll = false;
					diceGameModel.ShowAllDiceGameModels();
				}
				
	
				bool isHotDice = await TrySaveSelected(diceGameModel.GetSelected(), DiceGameUtils.GetCombinations(GetValues(diceGameModel.GetSelected())));
				tableModel.SetPreviewPoints(0);

				// Если все кубы забанкированы после сохранения, сбросить пул
				if (isHotDice)
				{
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
				
				var diceCombinationResult = DiceGameUtils.GetCombinations(GetValues(diceToRoll));
				var rollModifierContext = new DiceModifierContext(
					diceCombinationResult,
					diceToRoll,
					tableModel,
					diceGameModel,
					ModifierStage.Roll,
					run);
				
				await diceGameModel.ModifiersModel.PlayRollActions(rollModifierContext);

				if (diceCombinationResult.Combinations.Count == 0)
				{
					audioService.PlaySound(SoundNames.Fail);
					await UniTask.Delay(GlobalParameters.Delay);
					var roundEndContext = new DiceModifierContext(
						diceCombinationResult,
						diceToRoll,
						tableModel,
						diceGameModel,
						ModifierStage.RoundEnd,
						run);
					await diceGameModel.ModifiersModel.PlayRoundEndActions(roundEndContext);
					EndTurn(false);
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

		private async UniTask HandlePassAsync()
		{
			IsProcessing = true;

			try
			{
				diceGameModel.tableModel.DisableButtons();
				
				var selected = diceGameModel.GetSelected();
				var combo = DiceGameUtils.GetCombinations(GetValues(selected));
				var passModifierContext = new DiceModifierContext(
					combo,
					selected,
					tableModel,
					diceGameModel,
					ModifierStage.Pass,
					run);
				await diceGameModel.ModifiersModel.PlayPassActions(passModifierContext);
				await TrySaveSelected(selected, passModifierContext.CombinationResult);
				var roundEndContext = new DiceModifierContext(
					passModifierContext.CombinationResult,
					selected,
					tableModel,
					diceGameModel,
					ModifierStage.RoundEnd,
					run);
				await diceGameModel.ModifiersModel.PlayRoundEndActions(roundEndContext);
				EndTurn(true);
			}
			finally
			{
				diceGameModel.PassEnded();
				IsProcessing = false;
			}
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
			int points = DiceGameUtils.CalculateTotalScore(combinationResult);
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
			var tweens = new List<Tween>();
			
			foreach (var diceModel in diceGameModel.CurrentDiceModelList)
			{
				var pos = tableModel.GetFreeActivePosition();
				diceModel.SetSaved(false);
				diceModel.SetCurrentPosition(pos);

				var view = diceGameModel.ScreenDiceDict[diceModel];
				tweens.Add(view.MoveToPosition(pos.position));
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

		private DiceModel PickUpgradeDie(StraightUpgradeConfig upgradeConfig)
		{
			// MVP: choose randomly from currently equipped player dice (Option A).
			var pool = diceGameModel.PlayerDiceModelList;
			if (pool == null || pool.Count == 0)
			{
				return null;
			}

			int index = UnityEngine.Random.Range(0, pool.Count);
			return pool[index];
		}

		private string FormatOutcomeTable(StraightUpgradeOutcome[] outcomes)
		{
			if (outcomes == null || outcomes.Length == 0)
			{
				return "No outcomes configured.";
			}

			var sb = new StringBuilder();
			for (int i = 0; i < outcomes.Length; i++)
			{
				var o = outcomes[i];
				sb.Append($"Face {o.Face}: ΔMin {o.DeltaMinLen}, ΔMax {o.DeltaMaxLen}, ΔBonus {o.DeltaScoreBonus}");
				if (i < outcomes.Length - 1)
				{
					sb.Append(" | ");
				}
			}
			return sb.ToString();
		}

		private void NotifyAndLog(string message, Vector3? worldPos = null, float displaySeconds = 1.2f)
		{
			if (string.IsNullOrWhiteSpace(message))
			{
				return;
			}

			logger?.Log(message);
			ShowFloatingText(message, worldPos, displaySeconds);
		}

		private void ShowFloatingText(string text, Vector3? worldPos = null, float displaySeconds = 1.2f)
		{
			var position = worldPos ?? Vector3.zero;
			var go = new GameObject("UpgradeFloatingText");
			go.transform.position = position;
			go.AddComponent<FaceCameraBillboard>();
			var tmp = go.AddComponent<TextMeshPro>();
			tmp.text = text;
			tmp.enableAutoSizing = true;
			tmp.fontSizeMin = 0.15f;
			tmp.fontSizeMax = 0.5f;
			tmp.fontSize = 0.35f;
			tmp.alignment = TextAlignmentOptions.Center;
			tmp.color = Color.yellow;
			tmp.enableWordWrapping = true;
			tmp.sortingOrder = 50;
			tmp.lineSpacing = -10f;
			tmp.rectTransform.sizeDelta = new Vector2(3.2f, 1.6f);
			tmp.rectTransform.localScale = Vector3.one * 0.4f;

			tmp.DOFade(0f, 0.3f).SetDelay(displaySeconds);
			DOTween.Sequence()
				.AppendInterval(displaySeconds + 0.35f)
				.OnComplete(() => UnityEngine.Object.Destroy(go));
		}

		private class FaceCameraBillboard : MonoBehaviour
		{
			private void LateUpdate()
			{
				var cam = Camera.main;
				if (cam == null)
				{
					return;
				}

				transform.rotation = Quaternion.LookRotation(cam.transform.forward, cam.transform.up);
			}
		}

		private async UniTask TryTriggerUpgradeIfNeeded(DiceCombinationResult combinationResult)
		{
			if (combinationResult.Combinations == null || combinationResult.Combinations.Count == 0)
			{
				return;
			}

			// 1) Straight first
			var straightConfig = scoringService.GetStraightUpgradeConfig();
			if (straightConfig != null && straightConfig.Chance > 0f && combinationResult.Combinations.Any(e => IsStraightCombination(e.Combination)))
			{
				await HandleUpgradeForCombo("straight", straightConfig);
				return;
			}

			// 2) Of-a-kind
			var ofaConfig = scoringService.GetComboUpgradeConfig("ofakind");
			if (ofaConfig != null && ofaConfig.Chance > 0f && combinationResult.Combinations.Any(e => e.Combination == DiceCombination.ThreeOfAKind || e.Combination == DiceCombination.FourOfAKind || e.Combination == DiceCombination.FiveOfAKind || e.Combination == DiceCombination.SixOfAKind))
			{
				await HandleUpgradeForCombo("ofakind", ofaConfig);
				return;
			}
		}

		private async UniTask HandleUpgradeForCombo(string comboId, ComboUpgradeConfig upgradeConfig)
		{
			if (UnityEngine.Random.value > upgradeConfig.Chance)
			{
				if (upgradeConfig.Debug)
				{
					logger?.Log($"[Upgrade:{comboId}] Chance failed ({upgradeConfig.Chance:P0}).");
				}
				return;
			}

			var die = PickUpgradeDie(new StraightUpgradeConfig { Debug = upgradeConfig.Debug, Chance = upgradeConfig.Chance }); // reuse picker
			if (die == null)
			{
				logger?.LogWarning($"[Upgrade:{comboId}] No dice available for upgrade roll.");
				return;
			}

			diceGameModel.tableModel.DisableButtons();
			var announcePos = die.CurrentPosition != null
				? die.CurrentPosition.position
				: diceGameModel.tableModel?.GetFreeActivePosition()?.position ?? Vector3.zero;
			NotifyAndLog($"Upgrade opportunity: {comboId}", announcePos, 1.2f);
			logger?.Log($"[Upgrade:{comboId}] Triggered upgrade opportunity.");

			if (upgradeConfig.Debug)
			{
				var weights = die.Weights != null ? string.Join(",", die.Weights) : "null";
				var outcomes = scoringService.GetComboUpgradeOutcomes(comboId);
				var outcomeTable = outcomes != null
					? string.Join(", ", outcomes.Select(o => $"{o.Face}:Δmin{o.DeltaMin}/Δmax{o.DeltaMax}/Δb{o.DeltaScoreBonus}"))
					: "none";
				logger?.Log($"[Upgrade:{comboId}] Die {die.ConfigId}; chance={upgradeConfig.Chance:P0}; weights=[{weights}]; outcomes={outcomeTable}");
			}

			// Show outcome table before rolling (5s)
			var configuredOutcomes = scoringService.GetComboUpgradeOutcomes(comboId);
			var infoPos = die.CurrentPosition != null ? die.CurrentPosition.position : announcePos;
			NotifyAndLog($"Outcomes: {FormatOutcomeTable(configuredOutcomes)}", infoPos, 5f);
			await UniTask.Delay(5000);

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
				var before = scoringService.GetStraightState();
				var outcome = scoringService.ApplyStraightUpgradeOutcome(rolledFace, logger, null, run);
				var after = scoringService.GetStraightState();
				if (outcome != null)
				{
					var summary = $"Rolled {rolledFace}: Min {before.MinLen}->{after.MinLen}, Max {before.MaxLen}->{after.MaxLen}, Bonus {before.ScoreBonus}->{after.ScoreBonus}";
					NotifyAndLog(summary, infoPos, 5f);
					await UniTask.Delay(5000);
					logger?.Log($"[Upgrade:{comboId}] {summary} via die {die.ConfigId}");
				}
			}
			else
			{
				var before = scoringService.GetComboUpgradeState(comboId);
				var outcome = scoringService.ApplyGenericUpgradeOutcome(comboId, rolledFace, logger, null, run);
				var after = scoringService.GetComboUpgradeState(comboId);
				if (outcome != null && before != null && after != null)
				{
					var summary = $"Rolled {rolledFace}: Min {before.Min}->{after.Min}, Max {before.Max}->{after.Max}, Bonus {before.ScoreBonus}->{after.ScoreBonus}";
					NotifyAndLog(summary, infoPos, 5f);
					await UniTask.Delay(5000);
					logger?.Log($"[Upgrade:{comboId}] {summary} via die {die.ConfigId}");
				}
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


			if (DiceGameUtils.HasTrashInSelected(selectedValues))
			{
				tableModel.SetPreviewPoints(0);
			}
			else
			{
				var combo = DiceGameUtils.GetCombinations(selectedValues);
				tableModel.SetPreviewPoints(DiceGameUtils.CalculateTotalScore(combo));
			}


			diceGameModel.tableModel.SendUpdateUI();
		}
	}
}

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
		private readonly DiceScoringService scoringService;
		private TableModel tableModel => diceGameModel.tableModel;

		public bool IsProcessing { get; private set; }

		public DiceGameProcessController(
			ILoggerService logger,
			DiceGameModel diceGameModel,
			ICameraShakeService cameraShakeService,
			IAudioService audioService,
			DiceScoringService scoringService,
			Run run,
			GlobalNotificationService notificationService)
		{
			this.logger = logger;
			this.diceGameModel = diceGameModel;
			this.cameraShakeService = cameraShakeService;
			this.audioService = audioService;
			this.scoringService = scoringService;
			this.run = run;
			this.notificationService = notificationService;
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
					await diceGameModel.ModifiersModel.PlayRoundStartActions(roundStartContext);

					tableModel.isFirstRoll = false;
					diceGameModel.ShowAllDiceGameModels();
				}
				
	
				bool isHotDice = await TrySaveSelected(diceGameModel.GetSelected(), scoringService.Evaluate(GetValues(diceGameModel.GetSelected())));
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
				
				var diceCombinationResult = scoringService.Evaluate(GetValues(diceToRoll));
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
				var combo = scoringService.Evaluate(GetValues(selected));
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
			int points = scoringService.CalculateTotalScore(combinationResult);
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

		private GameObject ShowFloatingText(string text, Vector3? worldPos = null, float displaySeconds = 1.2f)
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

			return go;
		}

		private List<GameObject> ShowOutcomeRing(ComboUpgradeOutcome[] outcomes, Vector3 center, float radius = 1.2f, float displaySeconds = 5f)
		{
			var list = new List<GameObject>();
			if (outcomes == null || outcomes.Length == 0)
			{
				return list;
			}

			float step = 360f / outcomes.Length;
			for (int i = 0; i < outcomes.Length; i++)
			{
				float angle = step * i;
				var dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad));
				var pos = center + dir * radius + Vector3.up * 0.15f;
				var text = $"Face {outcomes[i].Face}\nΔMin {outcomes[i].DeltaMin}\nΔMax {outcomes[i].DeltaMax}\nΔB {outcomes[i].DeltaScoreBonus}";
				var go = ShowFloatingText(text, pos, displaySeconds);
				var tmp = go.GetComponent<TextMeshPro>();
				if (tmp != null)
				{
					tmp.fontSizeMin = 0.25f;
					tmp.fontSizeMax = 0.9f;
					tmp.fontSize = 0.6f;
					tmp.rectTransform.sizeDelta = new Vector2(5f, 2f);
					tmp.rectTransform.localScale = Vector3.one * 0.7f;
				}
				list.Add(go);
			}

			return list;
		}

		private void ShowOutcomeRingScreen(ComboUpgradeOutcome[] outcomes, Vector3 worldCenter, float radiusPx = 160f, float displaySeconds = 5f)
		{
			if (outcomes == null || outcomes.Length == 0)
			{
				return;
			}

			var canvasGo = new GameObject("UpgradeOutcomeCanvas");
			var canvas = canvasGo.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = 500;
			canvasGo.AddComponent<CanvasScaler>();
			canvasGo.AddComponent<GraphicRaycaster>();

			var cam = Camera.main;
			Vector3 screenCenter = cam != null ? cam.WorldToScreenPoint(worldCenter) : new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
			var canvasRect = canvas.GetComponent<RectTransform>();
			RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenCenter, null, out var localCenter);

			float step = 360f / outcomes.Length;
			for (int i = 0; i < outcomes.Length; i++)
			{
				float angle = step * i * Mathf.Deg2Rad;
				var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
				var screenPos = screenCenter + new Vector3(dir.x, dir.y, 0f) * radiusPx;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out var localPos);

				var go = new GameObject($"Outcome_{outcomes[i].Face}");
				go.transform.SetParent(canvas.transform, false);
				var rect = go.AddComponent<RectTransform>();
				rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
				rect.anchoredPosition = localPos;

				var tmp = go.AddComponent<TextMeshProUGUI>();
				tmp.text = $"Face {outcomes[i].Face}\nΔMin {outcomes[i].DeltaMin}\nΔMax {outcomes[i].DeltaMax}\nΔB {outcomes[i].DeltaScoreBonus}";
				tmp.fontSizeMin = 18f;
				tmp.fontSizeMax = 28f;
				tmp.enableAutoSizing = true;
				tmp.alignment = TextAlignmentOptions.Center;
				tmp.color = Color.yellow;
				tmp.rectTransform.sizeDelta = new Vector2(180f, 90f);
				tmp.lineSpacing = -8f;
			}

			DOTween.Sequence()
				.AppendInterval(displaySeconds)
				.OnComplete(() => UnityEngine.Object.Destroy(canvasGo));
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
				await HandleStraightUpgrade(straightConfig);
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
				var before = scoringService.GetStraightState();
				var outcome = scoringService.ApplyStraightUpgradeOutcome(rolledFace, logger, null, run);
				var after = scoringService.GetStraightState();
				if (outcome != null)
				{
					var summary = $"Rolled {rolledFace}: Min {before.MinLen}->{after.MinLen}, Max {before.MaxLen}->{after.MaxLen}, Bonus {before.ScoreBonus}->{after.ScoreBonus}";
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

		private async UniTask HandleStraightUpgrade(StraightUpgradeConfig upgradeConfig)
		{
			if (UnityEngine.Random.value > upgradeConfig.Chance)
			{
				if (upgradeConfig.Debug)
				{
					logger?.Log($"[Upgrade:straight] Chance failed ({upgradeConfig.Chance:P0}).");
				}
				return;
			}

			var die = PickUpgradeDie(upgradeConfig);
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

			var before = scoringService.GetStraightState();
			var outcome = scoringService.ApplyStraightUpgradeOutcome(rolledFace, logger, null, run);
			var after = scoringService.GetStraightState();
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


			if (scoringService.HasTrash(selectedValues))
			{
				tableModel.SetPreviewPoints(0);
			}
			else
			{
				var combo = scoringService.Evaluate(selectedValues);
				tableModel.SetPreviewPoints(scoringService.CalculateTotalScore(combo));
			}


			diceGameModel.tableModel.SendUpdateUI();
		}
	}
}

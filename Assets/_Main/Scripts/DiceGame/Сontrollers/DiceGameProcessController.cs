using System.Collections.Generic;
using System;
using _Main.Scripts.Core;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;
using PlatformCore.Services.Audio;

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
		private readonly IAsyncAwaiterPool turnFlowAwaiter;
		private TableModel tableModel => diceGameModel.tableModel;
		private Tween[] saveSelectedTweens = Array.Empty<Tween>();
		private Tween[] resetAllDiceTweens = Array.Empty<Tween>();
		private bool isWaitingForCommittedScoreChunks;
		private int pendingCommittedScore;

		public bool IsProcessing { get; private set; }

		public DiceGameProcessController(
			ILoggerService logger,
			DiceGameModel diceGameModel,
			ICameraShakeService cameraShakeService,
			IAudioService audioService,
			Run run,
			GlobalNotificationService notificationService,
			IAsyncAwaiterPool turnFlowAwaiter)
		{
			this.logger = logger;
			this.diceGameModel = diceGameModel;
			this.cameraShakeService = cameraShakeService;
			this.audioService = audioService;
			this.run = run;
			this.notificationService = notificationService;
			this.turnFlowAwaiter = turnFlowAwaiter ?? throw new ArgumentNullException(nameof(turnFlowAwaiter));
		}

		public void Activate()
		{
			logger?.Log("[DiceGameController] Activating...");

			diceGameModel.OnRollClicked += HandleRoll;
			diceGameModel.OnPassClicked += HandlePass;
			diceGameModel.DiceValuesChanged += OnDiceValuesChanged;
			diceGameModel.CombinationScoreChunkLanded += OnCombinationScoreChunkLandedHandler;

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
			diceGameModel.CombinationScoreChunkLanded -= OnCombinationScoreChunkLandedHandler;
			isWaitingForCommittedScoreChunks = false;
			pendingCommittedScore = 0;

			foreach (var diceModel in diceGameModel.CurrentDiceModelList)
			{
				diceModel.OnDiceChosenChanged -= UpdateUI;
			}
		}

		// === ОБРАБОТЧИКИ КНОПОК ===

		public void HandleRoll()
		{
			if (IsProcessing || diceGameModel.IsItemTargetingActive)
			{
				return;
			}

			_ = HandleRollAsync();
		}

		public void TryStartInitialRoll()
		{
			if (!CanStartInitialRoll())
			{
				return;
			}

			HandleRoll();
		}

		// ReSharper disable Unity.PerformanceAnalysis
		public async UniTask HandleRollAsync()
		{
			SetProcessingState(true);

			try
			{
				logger?.Log("[DiceGameController] Handle roll");
				logger?.Log(
					$"{BuildFlowContext("roll_start")} selected={diceGameModel.GetSelected().Length} " +
					$"unbanked={diceGameModel.GetUnbanked().Length} first_roll={tableModel.isFirstRoll}");

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
				var selectedDice = diceGameModel.GetSelected();
				bool isHotDice = await TrySaveSelected(
					selectedDice,
					activeScoringService.Evaluate(DiceGameUtils.GetDiceValues(selectedDice)));
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
				var diceToRoll = diceGameModel.GetUnbanked();
				var rollAnimationTasks = new UniTask[diceToRoll.Length];
				for (int i = 0; i < diceToRoll.Length; i++)
				{
					var dice = diceToRoll[i];
					dice.Roll();
					var view = diceGameModel.ScreenDiceDict[dice];
					rollAnimationTasks[i] = view.PlayRollAnimationAsync();
				}

				await UniTask.WhenAll(rollAnimationTasks);
				audioService.PlaySound(SoundNames.DiceDrop);
				cameraShakeService.ShakeAsync(0.4f, 0.065f).Forget();

				await UniTask.Delay(GlobalParameters.Delay / 2);
				
				var diceCombinationResult = activeScoringService.Evaluate(DiceGameUtils.GetDiceValues(diceToRoll));
				var rollModifierContext = new DiceModifierContext(
					diceCombinationResult,
					diceToRoll,
					tableModel,
					diceGameModel,
					ModifierStage.Roll,
					run);

				await diceGameModel.GetCurrentModifiersModel().PlayRollActions(rollModifierContext);
				var evaluatedScore = activeScoringService.CalculateTotalScore(diceCombinationResult);
				logger?.Log(
					$"{BuildFlowContext("roll_eval")} combo_count={diceCombinationResult.Combinations.Count} " +
					$"evaluated_score={evaluatedScore}");

				if (diceCombinationResult.Combinations.Count == 0)
				{
					await HandleFailedRollAsync(diceCombinationResult, diceToRoll);
				}
			}
			finally
			{
				try
				{
					await WaitTurnFlowAsync();
				}
				finally
				{
					SetProcessingState(false);
				}

				diceGameModel.RollEnded();
				await TryStartInitialRollAfterProcessingAsync();
			}
		}

		private void HandlePass()
		{
			if (IsProcessing || diceGameModel.IsItemTargetingActive)
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
			SetProcessingState(true);

			try
			{
				diceGameModel.tableModel.DisableButtons();
				
				var selected = diceGameModel.GetSelected();
				logger?.Log($"{BuildFlowContext("pass_start")} selected={selected.Length}");
				var activeScoringService = diceGameModel.GetCurrentScoringService();
				var combo = activeScoringService.Evaluate(DiceGameUtils.GetDiceValues(selected));
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
				logger?.Log($"{BuildFlowContext("pass_commit")} selected={selected.Length}");
				EndTurn(true);
			}
			finally
			{
				try
				{
					await WaitTurnFlowAsync();
				}
				finally
				{
					SetProcessingState(false);
				}

				diceGameModel.PassEnded();
				await TryStartInitialRollAfterProcessingAsync();
			}
		}

		private void SetProcessingState(bool value)
		{
			if (IsProcessing == value)
			{
				return;
			}

			IsProcessing = value;
			if (value)
			{
				diceGameModel.BeginDiceAnimation();
			}
			else
			{
				diceGameModel.EndDiceAnimation();
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
			var diceCombinationResult = activeScoringService.Evaluate(DiceGameUtils.GetDiceValues(diceToCheck));
			if (diceCombinationResult.Combinations.Count == 0)
			{
				await HandleFailedRollAsync(diceCombinationResult, diceToCheck);
			}
		}

		private async UniTask HandleFailedRollAsync(DiceCombinationResult diceCombinationResult, DiceModel[] diceToRoll)
		{
			logger?.Log(
				$"{BuildFlowContext("roll_failed")} combo_count={diceCombinationResult.Combinations?.Count ?? 0} " +
				$"unbanked={diceToRoll?.Length ?? 0}");
			audioService.PlaySound(SoundNames.Fail);
			if (notificationService != null)
			{
				await notificationService.ShowBannerAsync("dice_banner_failed", 1.1f, true);
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
			logger?.Log($"{BuildFlowContext("turn_end")} success={success}");
			diceGameModel.EndTurn(success);
			audioService.PlaySound(SoundNames.TurnChange);
		}

		public async UniTask<bool> TrySaveSelected(DiceModel[] selected, DiceCombinationResult combinationResult)
		{
			var activeScoringService = diceGameModel.GetCurrentScoringService();
			int points = activeScoringService.CalculateTotalScore(combinationResult);
			if (points <= 0)
			{
				logger?.Log(
					$"{BuildFlowContext("save_selected_skipped")} selected={selected.Length} " +
					$"combo_count={combinationResult.Combinations?.Count ?? 0}");
				return false;
			}

			if (saveSelectedTweens.Length < selected.Length)
			{
				saveSelectedTweens = new Tween[selected.Length];
			}

			var tweens = saveSelectedTweens;
			var tweenCount = 0;
			foreach (var diceModel in selected)
			{
				diceModel.SetSaved(true);
				diceModel.SetChosen(false);

				var position = tableModel.GetFreeBankedPosition();
				diceModel.SetCurrentPosition(position);
				var view = diceGameModel.ScreenDiceDict[diceModel];
				view.transform.SetParent(position);
				view.ResetYRotation();
				tweens[tweenCount] = view.MoveToPosition(position.position);
				tweenCount++;
			}

			await UniTaskUtils.WaitAllTweens(tweens, tweenCount);
			var committedSnapshot = DiceCombinationCardsSnapshotBuilder.Build(combinationResult, activeScoringService);
			pendingCommittedScore = points;
			isWaitingForCommittedScoreChunks = true;
			diceGameModel.PublishCombinationCommitted(committedSnapshot);

			await WaitTurnFlowAsync();
			isWaitingForCommittedScoreChunks = false;

			if (pendingCommittedScore > 0)
			{
				tableModel.AddTurnPoints(pendingCommittedScore);
			}

			pendingCommittedScore = 0;
			logger?.Log(
				$"{BuildFlowContext("save_selected")} selected={selected.Length} " +
				$"combo_count={combinationResult.Combinations?.Count ?? 0} added_points={points}");

			diceGameModel.RequestUpgrade(combinationResult);
			await WaitTurnFlowAsync();

			return diceGameModel.AllBanked();
		}

		private async UniTask ResetAllDiceToActiveAsync()
		{
			tableModel.ResetAllPositions();
			if (resetAllDiceTweens.Length < diceGameModel.CurrentDiceModelList.Count)
			{
				resetAllDiceTweens = new Tween[diceGameModel.CurrentDiceModelList.Count];
			}

			var tweens = resetAllDiceTweens;
			var tweenCount = 0;
			
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
					view.transform.SetParent(pos);
					tweens[tweenCount] = view.MoveToPosition(pos.position);
					tweenCount++;
				}
				else
				{
					logger?.LogWarning($"[DiceGameController] Missing dice view for model {diceModel?.ConfigId} while resetting.");
				}
			}

			await UniTaskUtils.WaitAllTweens(tweens, tweenCount);
		}

		private void OnCombinationScoreChunkLandedHandler(int scoreChunk)
		{
			if (!isWaitingForCommittedScoreChunks)
			{
				return;
			}

			if (scoreChunk <= 0 || pendingCommittedScore <= 0)
			{
				return;
			}

			var appliedScore = scoreChunk > pendingCommittedScore ? pendingCommittedScore : scoreChunk;
			pendingCommittedScore -= appliedScore;
			tableModel.AddTurnPoints(appliedScore);
		}

		private void UpdateUI()
		{
			if (diceGameModel.IsDiceAnimationInProgress)
			{
				return;
			}

			if (tableModel.isFirstRoll)
			{
				diceGameModel.HideAllDiceGameModels();
			}
			else
			{
				diceGameModel.ShowAllDiceGameModels();
			}

			var selectedDice = diceGameModel.GetSelected();
			var selectedValues = DiceGameUtils.GetDiceValues(selectedDice);

			var activeScoringService = diceGameModel.GetCurrentScoringService();
			var previewSnapshot = DiceCombinationCardsSnapshot.Empty;
			var combo = activeScoringService.Evaluate(selectedValues);

			if (DiceGameUtils.HasRemainingTrash(combo.RemainingCounts))
			{
				tableModel.SetPreviewPoints(0);
			}
			else
			{
				var previewPoints = activeScoringService.CalculateTotalScore(combo);
				tableModel.SetPreviewPoints(previewPoints);
				if (previewPoints > 0 && diceGameModel.IsPlayerTurn)
				{
					previewSnapshot = DiceCombinationCardsSnapshotBuilder.Build(combo, activeScoringService);
				}
			}

			diceGameModel.PublishCombinationPreview(previewSnapshot);

			diceGameModel.tableModel.SendUpdateUI();
		}

		private UniTask WaitTurnFlowAsync()
		{
			return turnFlowAwaiter.WaitForEmptyAsync();
		}

		private string BuildFlowContext(string stage)
		{
			var currentTableModel = tableModel;
			var playerBanked = currentTableModel != null ? currentTableModel.PlayerBankedPoints : 0;
			var enemyBanked = currentTableModel != null ? currentTableModel.EnemyBankedPoints : 0;
			var turnPoints = currentTableModel != null ? currentTableModel.TurnPoints : 0;
			var side = diceGameModel.IsPlayerTurn ? "player" : "enemy";

			return $"[DiceMatchFlow] stage={stage} side={side} turn={diceGameModel.CurrentTurn} " +
			       $"player_banked={playerBanked} enemy_banked={enemyBanked} " +
			       $"target={diceGameModel.TargetPoints} turn_points={turnPoints}";
		}

		private bool CanStartInitialRoll()
		{
			if (IsProcessing)
			{
				return false;
			}

			if (diceGameModel.DiceGameState != DiceGameState.GAME)
			{
				return false;
			}

			if (!diceGameModel.IsPlayerTurn)
			{
				return false;
			}

			return tableModel.isFirstRoll;
		}

		private async UniTask TryStartInitialRollAfterProcessingAsync()
		{
			if (!CanStartInitialRoll())
			{
				return;
			}

			await WaitTurnFlowAsync();

			if (!CanStartInitialRoll())
			{
				return;
			}

			HandleRoll();
		}
	}
}

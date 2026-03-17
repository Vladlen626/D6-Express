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

		public void TryStartInitialRoll()
		{
			if (IsProcessing)
			{
				return;
			}

			if (diceGameModel.DiceGameState != DiceGameState.GAME)
			{
				return;
			}

			if (!diceGameModel.IsPlayerTurn)
			{
				return;
			}

			if (!tableModel.isFirstRoll)
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
					activeScoringService.Evaluate(GetValues(selectedDice)));
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
				SetProcessingState(false);
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
			SetProcessingState(true);

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
				SetProcessingState(false);
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
			diceGameModel.EndTurn(success);
			audioService.PlaySound(SoundNames.TurnChange);

			if (diceGameModel.IsPlayerTurn)
			{
				TryStartInitialRoll();
			}

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

			diceGameModel.PublishCombinationPreview(DiceCombinationCardsSnapshot.Empty);

			var tweens = new Tween[selected.Length];
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
			diceGameModel.PublishCombinationCommitted(committedSnapshot);

			await WaitTurnFlowAsync();

			tableModel.AddTurnPoints(points);

			diceGameModel.RequestUpgrade(combinationResult);
			await WaitTurnFlowAsync();

			UpdateUI();

			return diceGameModel.AllBanked();
		}

		private async UniTask ResetAllDiceToActiveAsync()
		{
			tableModel.ResetAllPositions();
			var tweens = new Tween[diceGameModel.CurrentDiceModelList.Count];
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
			var selectedValues = GetValues(selectedDice);

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
	}
}

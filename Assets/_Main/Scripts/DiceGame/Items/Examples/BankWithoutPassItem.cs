using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Instant click item: banks current preview selection without ending the turn.
	/// </summary>
	public class BankWithoutPassItem : ModifierItemBase, IGameModelBoundItem, IModifierItemViewProvider
	{
		private readonly DiceScoringService scoringService;
		private readonly ItemView customPrefab;
		private DiceGameModel boundGameModel;

		public BankWithoutPassItem(string id, DiceScoringService scoringService, ItemView prefabOverride = null)
			: base(id, id, DiceItemActivationType.ClickToActivate)
		{
			this.scoringService = scoringService;
			customPrefab = prefabOverride;
		}

		public override string InvalidActivationNotificationKey => GlobalConstants.Localization.ItemActivationOnlyGame;

		public override bool IsActivationAllowed(DiceGameState gameState)
		{
			return gameState == DiceGameState.GAME;
		}

		protected override bool OnClick()
		{
			if (State != DiceItemState.Ready || boundGameModel == null || boundGameModel.tableModel == null)
			{
				return false;
			}

			SetState(DiceItemState.Armed);
			NotifyActivationStarted();

			if (!TryBuildBankRequest(out var bankRequest))
			{
				SetState(DiceItemState.Ready);
				return false;
			}

			ExecuteBankCurrentPreviewAsync(bankRequest).Forget();
			return true;
		}

		private bool TryBuildBankRequest(out BankRequest bankRequest)
		{
			bankRequest = default;
			var activeScoringService = GetScoringServiceOrThrow();

			var selected = boundGameModel.GetSelected();
			if (selected.Length == 0)
			{
				return false;
			}

			var values = DiceGameUtils.GetDiceValues(selected);
			var combo = activeScoringService.Evaluate(values);
			if (DiceGameUtils.HasRemainingTrash(combo.RemainingCounts))
			{
				return false;
			}

			var points = activeScoringService.CalculateTotalScore(combo);
			if (points <= 0)
			{
				return false;
			}

			bankRequest = new BankRequest(selected, combo, points);
			return true;
		}

		private async UniTaskVoid ExecuteBankCurrentPreviewAsync(BankRequest bankRequest)
		{
			if (boundGameModel == null || boundGameModel.tableModel == null)
			{
				SetState(DiceItemState.Ready);
				return;
			}

			var gameModel = boundGameModel;
			var animationStarted = false;

			try
			{
				gameModel.BeginDiceAnimation();
				animationStarted = true;

				await SaveSelectedAsync(gameModel, bankRequest);

				NotifyEffectApplied();
				Consume();
			}
			catch (Exception ex)
			{
				Debug.LogError($"[BankWithoutPassItem] Failed to bank preview: {ex}");
				if (State != DiceItemState.Consumed)
				{
					SetState(DiceItemState.Ready);
				}
			}
			finally
			{
				if (animationStarted && gameModel.IsDiceAnimationInProgress)
				{
					gameModel.EndDiceAnimation();
				}
			}
		}

		private async UniTask SaveSelectedAsync(DiceGameModel gameModel, BankRequest bankRequest)
		{
			var activeScoringService = GetScoringServiceOrThrow();
			var table = gameModel.tableModel;
			gameModel.PublishCombinationPreview(DiceCombinationCardsSnapshot.Empty);

			var tweens = new Tween[bankRequest.Selected.Length];
			var tweenCount = 0;
			for (int i = 0; i < bankRequest.Selected.Length; i++)
			{
				var diceModel = bankRequest.Selected[i];
				if (diceModel == null)
				{
					throw new InvalidOperationException("[BankWithoutPassItem] Selected dice list contains a null entry.");
				}

				diceModel.SetSaved(true);
				diceModel.SetChosen(false);

				if (!gameModel.ScreenDiceDict.TryGetValue(diceModel, out var view) || !view)
				{
					throw new InvalidOperationException(
						$"[BankWithoutPassItem] Missing dice view for model '{diceModel.ConfigId}' while saving selected dice.");
				}

				var position = table.GetFreeBankedPosition();
				if (!position)
				{
					throw new InvalidOperationException("[BankWithoutPassItem] No free banked position while saving selected dice.");
				}

				diceModel.SetCurrentPosition(position);

				view.transform.SetParent(position);
				view.ResetYRotation();
				tweens[tweenCount] = view.MoveToPosition(position.position);
				tweenCount++;
			}

			await UniTaskUtils.WaitAllTweens(tweens, tweenCount);

			var committedSnapshot = DiceCombinationCardsSnapshotBuilder.Build(bankRequest.Combination, activeScoringService);
			gameModel.PublishCombinationCommitted(committedSnapshot);

			await WaitTurnFlowAsync(gameModel);

			table.AddTurnPoints(bankRequest.Points);
			gameModel.RequestUpgrade(bankRequest.Combination);
			await WaitTurnFlowAsync(gameModel);

			if (gameModel.AllBanked())
			{
				await ResetAllDiceToActiveAsync(gameModel);
			}

			UpdatePreview();
			gameModel.NotifyDiceValuesChanged();
		}

		private async UniTask ResetAllDiceToActiveAsync(DiceGameModel gameModel)
		{
			var table = gameModel.tableModel;
			table.ResetAllPositions();
			var diceList = gameModel.CurrentDiceModelList;
			var tweens = new Tween[diceList.Count];
			var tweenCount = 0;

			for (int i = 0; i < diceList.Count; i++)
			{
				var diceModel = diceList[i];
				if (diceModel == null)
				{
					throw new InvalidOperationException("[BankWithoutPassItem] Current dice list contains a null entry.");
				}

				if (!gameModel.ScreenDiceDict.TryGetValue(diceModel, out var view) || !view)
				{
					throw new InvalidOperationException(
						$"[BankWithoutPassItem] Missing dice view for model '{diceModel.ConfigId}' while resetting dice.");
				}

				var position = table.GetFreeActivePosition();
				if (!position)
				{
					throw new InvalidOperationException("[BankWithoutPassItem] No free active position while resetting dice.");
				}

				diceModel.SetSaved(false);
				diceModel.SetChosen(false);
				diceModel.SetCurrentPosition(position);
				view.transform.SetParent(position);
				tweens[tweenCount] = view.MoveToPosition(position.position);
				tweenCount++;
			}

			await UniTaskUtils.WaitAllTweens(tweens, tweenCount);
			gameModel.ResetAllDices();
		}

		private void UpdatePreview()
		{
			if (boundGameModel?.tableModel == null)
			{
				return;
			}

			var activeScoringService = GetScoringServiceOrThrow();
			var selected = boundGameModel.GetSelected();
			var values = DiceGameUtils.GetDiceValues(selected);
			var combo = activeScoringService.Evaluate(values);

			if (DiceGameUtils.HasRemainingTrash(combo.RemainingCounts))
			{
				boundGameModel.tableModel.SetPreviewPoints(0);
			}
			else
			{
				var total = activeScoringService.CalculateTotalScore(combo);
				boundGameModel.tableModel.SetPreviewPoints(total);
			}

			boundGameModel.tableModel.SendUpdateUI();
		}

		private static UniTask WaitTurnFlowAsync(DiceGameModel gameModel)
		{
			var turnFlowAwaiter = gameModel.TurnFlowAwaiter;
			if (turnFlowAwaiter == null)
			{
				throw new InvalidOperationException("[BankWithoutPassItem] TurnFlowAwaiter is not configured.");
			}

			return turnFlowAwaiter.WaitForEmptyAsync();
		}

		private DiceScoringService GetScoringServiceOrThrow()
		{
			if (scoringService != null)
			{
				return scoringService;
			}

			if (boundGameModel != null)
			{
				var modelScoringService = boundGameModel.GetCurrentScoringService();
				if (modelScoringService != null)
				{
					return modelScoringService;
				}
			}

			throw new InvalidOperationException(
				"[BankWithoutPassItem] ScoringService is not configured. " +
				"Pass it via ModifierItemFactory or bind a game model with configured scoring.");
		}

		public override UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			return UniTask.CompletedTask;
		}

		public void OnAddedToGameModel(DiceGameModel gameModel)
		{
			boundGameModel = gameModel;
		}

		public void OnRemovedFromGameModel(DiceGameModel gameModel)
		{
			if (object.ReferenceEquals(boundGameModel, gameModel))
			{
				boundGameModel = null;
			}
		}

		public override void ResetItem()
		{
			base.ResetItem();
			boundGameModel = null;
		}

		public ItemView GetViewPrefab() => customPrefab;

		private readonly struct BankRequest
		{
			public DiceModel[] Selected { get; }
			public DiceCombinationResult Combination { get; }
			public int Points { get; }

			public BankRequest(DiceModel[] selected, DiceCombinationResult combination, int points)
			{
				Selected = selected;
				Combination = combination;
				Points = points;
			}
		}
	}
}

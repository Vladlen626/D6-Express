using System.Collections.Generic;
using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Clickable item: arm it, then the next clicked (unsaved) die rerolls immediately.
	/// After use it is consumed and cannot be activated again.
	/// </summary>
	public class RerollSelectedItem : ModifierItemBase, IOnPassModifier, IOnRoundStartModifier, IModifierItemViewProvider
	{
		private readonly DiceScoringService scoringService;
		private readonly ItemView customPrefab;
		private DiceGameModel boundGameModel;
		private readonly Dictionary<DiceView, UnityAction> clickHandlers = new();
		private bool handlersAttached;

		public RerollSelectedItem(string id, DiceScoringService scoringService, int cooldownPasses = 2, ItemView prefabOverride = null)
			: base(id, id, DiceItemActivationType.ClickToActivate)
		{
			this.scoringService = scoringService;
			_ = cooldownPasses;
			customPrefab = prefabOverride;
		}

		public override bool BlocksGameplayWhileArmed => true;

		public override string InvalidActivationNotificationKey => GlobalConstants.Localization.ItemActivationOnlyGame;

		public override bool IsActivationAllowed(DiceGameState gameState)
		{
			return gameState == DiceGameState.GAME;
		}

		public override async UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			if (State == DiceItemState.Consumed)
			{
				await UniTask.CompletedTask;
				return;
			}

			TryAttachDiceHandlers(modifierContext.DiceGameModel);

			switch (modifierContext.Stage)
			{
				case ModifierStage.RoundStart:
					// If a new round starts while armed, keep it armed.
					break;
			}

			await UniTask.CompletedTask;
		}

		protected override bool OnClick()
		{
			if (State != DiceItemState.Ready)
			{
				return false;
			}

			SetState(DiceItemState.Armed);
			NotifyActivationStarted();
			return true;
		}

		private async UniTask ApplyRerollAsync(DiceModifierContext context)
		{
			if (context.Dice == null || context.Dice.Length == 0)
			{
				return;
			}

			List<UniTask> animations = null;

			foreach (var dice in context.Dice)
			{
				if (dice == null || dice.IsSaved || DiceGameUtils.IsDiceBanked(dice, context.Table))
				{
					continue;
				}

				dice.Roll();

				if (context.DiceGameModel != null &&
				    context.DiceGameModel.ScreenDiceDict.TryGetValue(dice, out var view))
				{
					animations ??= new List<UniTask>();
					animations.Add(view.PlayRollAnimationAsync(0.35f));
				}
			}

			if (animations is { Count: > 0 })
			{
				await UniTask.WhenAll(animations);
			}

			// Refresh combinations to reflect the new dice values.
			var recomputed = scoringService.Evaluate(DiceGameUtils.GetDiceValues(context.Dice));
			var targetList = context.CombinationResult.Combinations;
			targetList.Clear();
			targetList.AddRange(recomputed.Combinations);
		}

		private async void OnDiceClickedAsync(DiceModel model, DiceView view)
		{
			if (State != DiceItemState.Armed || model == null || boundGameModel == null)
			{
				return;
			}

			if (model.IsSaved || DiceGameUtils.IsDiceBanked(model, boundGameModel.tableModel))
			{
				return;
			}

			model.Roll();
			if (view)
			{
				await view.PlayRollAnimationAsync(0.35f);
			}

			UpdatePreview();
			boundGameModel?.NotifyDiceValuesChanged();
			NotifyEffectApplied();
			ConsumeAndDeactivate();
		}

		private void TryAttachDiceHandlers(DiceGameModel gameModel)
		{
			if (gameModel == null || gameModel.ScreenDiceDict == null)
			{
				return;
			}

			if (handlersAttached)
			{
				// If this is a new game instance or the dice views changed, reattach.
				if (!ReferenceEquals(boundGameModel, gameModel) || !HasSameDiceViews(gameModel))
				{
					DetachDiceHandlers();
				}
				else
				{
					return;
				}
			}

			boundGameModel = gameModel;

			foreach (var kv in gameModel.ScreenDiceDict)
			{
				var model = kv.Key;
				var view = kv.Value;
				if (!view)
				{
					continue;
				}

				UnityAction listener = () => OnDiceClickedAsync(model, view);
				view.OnDiceClicked.AddListener(listener);
				clickHandlers[view] = listener;
			}

			handlersAttached = true;
		}

		private bool HasSameDiceViews(DiceGameModel gameModel)
		{
			var attachableViewCount = 0;
			foreach (var kv in gameModel.ScreenDiceDict)
			{
				var view = kv.Value;
				if (!view)
				{
					continue;
				}

				attachableViewCount++;
				if (!clickHandlers.ContainsKey(view))
				{
					return false;
				}
			}

			return clickHandlers.Count == attachableViewCount;
		}

		private void DetachDiceHandlers()
		{
			foreach (var kv in clickHandlers)
			{
				if (kv.Key)
				{
					kv.Key.OnDiceClicked.RemoveListener(kv.Value);
				}
			}

			clickHandlers.Clear();
			handlersAttached = false;
			boundGameModel = null;
		}

		private void UpdatePreview()
		{
			if (boundGameModel?.tableModel == null)
			{
				return;
			}

			var selected = boundGameModel.GetSelected();
			var values = DiceGameUtils.GetDiceValues(selected);

			if (scoringService.HasTrash(values))
			{
				boundGameModel.tableModel.SetPreviewPoints(0);
			}
			else
			{
				var combo = scoringService.Evaluate(values);
				var total = scoringService.CalculateTotalScore(combo);
				boundGameModel.tableModel.SetPreviewPoints(total);
			}

			boundGameModel.tableModel.SendUpdateUI();
		}

		private void ConsumeAndDeactivate()
		{
			Consume();
			DetachDiceHandlers();
		}

		public override void ResetItem()
		{
			base.ResetItem();
			DetachDiceHandlers();
		}

		public ItemView GetViewPrefab() => customPrefab;
	}
}

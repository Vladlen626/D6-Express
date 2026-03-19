using System.Collections.Generic;
using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Click-to-activate item: select N dice (default 3); after the Nth selection, selected dice
	/// advance their face by +1 (wrapping 6 -> 1). Then it is consumed.
	/// </summary>
	public class StepUpItem : ModifierItemBase, IOnPassModifier, IOnRoundStartModifier, IModifierItemViewProvider
	{
		private readonly DiceScoringService scoringService;
		private readonly int selectionTarget;
		private readonly ItemView customPrefab;

		private readonly HashSet<DiceModel> selectedDice = new();
		private readonly Dictionary<DiceView, UnityAction> clickHandlers = new();

		private DiceGameModel boundGameModel;
		private bool handlersAttached;
		private bool isProcessing;

		public StepUpItem(string id, DiceScoringService scoringService, int selectionCount = 3, int? cooldownPasses = null, ItemView prefabOverride = null)
			: base(id, id, DiceItemActivationType.ClickToActivate)
		{
			this.scoringService = scoringService;
			selectionTarget = Mathf.Max(1, selectionCount);
			_ = cooldownPasses;
			customPrefab = prefabOverride;
		}

		public override bool BlocksGameplayWhileArmed => true;

		public override string InvalidActivationNotificationKey => GlobalConstants.Localization.ItemActivationOnlyGame;

		public override bool IsActivationAllowed(DiceGameState gameState)
		{
			return gameState == DiceGameState.GAME;
		}

		public override UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			if (State == DiceItemState.Consumed)
			{
				return UniTask.CompletedTask;
			}

			TryAttachDiceHandlers(modifierContext.DiceGameModel);
			return UniTask.CompletedTask;
		}

		protected override bool OnClick()
		{
			if (State != DiceItemState.Ready)
			{
				return false;
			}

			selectedDice.Clear();
			SetState(DiceItemState.Armed);
			NotifyActivationStarted();
			return true;
		}

		private void OnDiceClicked(DiceModel model)
		{
			if (State != DiceItemState.Armed || isProcessing || model == null || boundGameModel == null)
			{
				return;
			}

			if (model.IsSaved || DiceGameUtils.IsDiceBanked(model, boundGameModel.tableModel))
			{
				return;
			}

			if (!selectedDice.Add(model))
			{
				selectedDice.Remove(model);
				return;
			}

			if (selectedDice.Count >= selectionTarget)
			{
				isProcessing = true;
				try
				{
					ApplyStep();
				}
				finally
				{
					isProcessing = false;
				}
			}
		}

		private void ApplyStep()
		{
			var diceList = boundGameModel?.CurrentDiceModelList;
			if (diceList == null || diceList.Count == 0)
			{
				selectedDice.Clear();
				return;
			}

			var availableDice = new HashSet<DiceModel>(diceList);
			var targetDice = new List<DiceModel>(selectedDice.Count);
			foreach (var dice in selectedDice)
			{
				if (dice == null || !availableDice.Contains(dice))
				{
					continue;
				}

				if (dice.IsSaved || DiceGameUtils.IsDiceBanked(dice, boundGameModel.tableModel))
				{
					continue;
				}

				targetDice.Add(dice);
			}

			if (targetDice.Count == 0)
			{
				selectedDice.Clear();
				return;
			}

			foreach (var dice in targetDice)
			{
				var nextValue = GetNextValue(dice.CurrentValue);
				dice.SetValue(nextValue);
			}

			UpdatePreview();
			boundGameModel?.NotifyDiceValuesChanged();
			NotifyEffectApplied();
			Consume();
			DetachDiceHandlers();
		}

		private static int GetNextValue(int current)
		{
			if (current < 1 || current > 6)
			{
				return 1;
			}

			return current == 6 ? 1 : current + 1;
		}

		private void TryAttachDiceHandlers(DiceGameModel gameModel)
		{
			if (gameModel == null || gameModel.ScreenDiceDict == null)
			{
				return;
			}

			if (handlersAttached)
			{
				if (!ReferenceEquals(boundGameModel, gameModel) || !HasSameDiceViews(gameModel))
				{
					DetachDiceHandlers();
					selectedDice.Clear();
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

				UnityAction listener = () => OnDiceClicked(model);
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
				boundGameModel.tableModel.SetPreviewPoints(scoringService.CalculateTotalScore(combo));
			}

			boundGameModel.tableModel.SendUpdateUI();
		}

		protected override void OnCancelArmedTargeting()
		{
			selectedDice.Clear();
			isProcessing = false;
		}

		public override void ResetItem()
		{
			base.ResetItem();
			selectedDice.Clear();
			isProcessing = false;
			DetachDiceHandlers();
		}

		public ItemView GetViewPrefab() => customPrefab;
	}
}

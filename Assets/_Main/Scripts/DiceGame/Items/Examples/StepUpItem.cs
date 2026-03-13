using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Click-to-activate item: select N dice (default 3); after the Nth selection, selected dice
	/// advance their face by +1 (wrapping 6 -> 1). Then it goes on cooldown for N passes.
	/// </summary>
	public class StepUpItem : ModifierItemBase, IOnPassModifier, IOnRoundStartModifier, IModifierItemViewProvider
	{
		private readonly DiceScoringService scoringService;
		private readonly int selectionTarget;
		private readonly int cooldownLengthInPasses;
		private readonly ItemView customPrefab;

		private readonly HashSet<DiceModel> selectedDice = new();
		private readonly Dictionary<DiceView, UnityAction> clickHandlers = new();

		private DiceGameModel boundGameModel;
		private bool handlersAttached;
		private bool isProcessing;
		private int cooldownRemaining;

		public StepUpItem(string id, DiceScoringService scoringService, int selectionCount = 3, int? cooldownPasses = null, ItemView prefabOverride = null)
			: base(id, id, DiceItemActivationType.ClickToActivate)
		{
			this.scoringService = scoringService;
			selectionTarget = Mathf.Max(1, selectionCount);
			cooldownLengthInPasses = Mathf.Max(1, cooldownPasses ?? selectionTarget);
			customPrefab = prefabOverride;
		}

		public override async UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			TryAttachDiceHandlers(modifierContext.DiceGameModel);

			switch (modifierContext.Stage)
			{
				case ModifierStage.RoundStart:
					// Keep armed state if the player queued the item; just ensure handlers are bound.
					break;
				case ModifierStage.Pass:
					TickCooldown();
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

			selectedDice.Clear();
			SetState(DiceItemState.Armed);
			return true;
		}

		private async void OnDiceClickedAsync(DiceModel model)
		{
			if (State != DiceItemState.Armed || isProcessing || model == null || model.IsSaved)
			{
				return;
			}

			if (!selectedDice.Add(model))
			{
				return;
			}

			if (selectedDice.Count >= selectionTarget)
			{
				isProcessing = true;
				await ApplyStepAsync();
				isProcessing = false;
			}
		}

		private async UniTask ApplyStepAsync()
		{
			var diceList = boundGameModel?.CurrentDiceModelList;
			if (diceList == null || diceList.Count == 0)
			{
				BeginCooldown();
				return;
			}

			var availableDice = new HashSet<DiceModel>(diceList);
			var targetDice = new List<DiceModel>(selectedDice.Count);
			foreach (var dice in selectedDice)
			{
				if (dice != null && availableDice.Contains(dice))
				{
					targetDice.Add(dice);
				}
			}

			if (targetDice.Count == 0)
			{
				BeginCooldown();
				return;
			}

			foreach (var dice in targetDice)
			{
				var nextValue = GetNextValue(dice.CurrentValue);
				dice.SetValue(nextValue);
			}

			UpdatePreview();
			boundGameModel?.NotifyDiceValuesChanged();
			BeginCooldown();
		}

		private static int GetNextValue(int current)
		{
			if (current < 1 || current > 6)
			{
				return 1;
			}

			return current == 6 ? 1 : current + 1;
		}

		private void BeginCooldown()
		{
			cooldownRemaining = cooldownLengthInPasses;
			selectedDice.Clear();
			StartCooldown();
		}

		private void TickCooldown()
		{
			if (State != DiceItemState.Cooldown)
			{
				return;
			}

			if (cooldownRemaining > 0)
			{
				cooldownRemaining--;
			}

			if (cooldownRemaining <= 0)
			{
				SetState(DiceItemState.Ready);
			}
		}

		private void TryAttachDiceHandlers(DiceGameModel gameModel)
		{
			if (gameModel == null || gameModel.ScreenDiceDict == null)
			{
				return;
			}

			if (handlersAttached)
			{
				if (!ReferenceEquals(boundGameModel, gameModel) ||
				    clickHandlers.Count != gameModel.ScreenDiceDict.Count)
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

				UnityAction listener = () => OnDiceClickedAsync(model);
				view.OnDiceClicked.AddListener(listener);
				clickHandlers[view] = listener;
			}

			handlersAttached = true;
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

		public override void ResetItem()
		{
			base.ResetItem();
			cooldownRemaining = 0;
			selectedDice.Clear();
			isProcessing = false;
			DetachDiceHandlers();
		}

		public ItemView GetViewPrefab() => customPrefab;
	}
}

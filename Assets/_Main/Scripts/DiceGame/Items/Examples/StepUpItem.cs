using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Click-to-activate item: select N dice (default 3); after the Nth selection, every die
	/// on the board advances its face by +1 (wrapping 6 -> 1). Then it goes on cooldown for N passes.
	/// </summary>
	public class StepUpItem : DiceItemBase, IOnPassModifier, IOnRoundStartModifier, IDiceItemViewProvider
	{
		private readonly int selectionTarget;
		private readonly int cooldownLengthInPasses;
		private readonly DiceItemView customPrefab;

		private readonly HashSet<DiceModel> selectedDice = new();
		private readonly Dictionary<DiceView, UnityAction> clickHandlers = new();

		private DiceGameModel boundGameModel;
		private bool handlersAttached;
		private bool isProcessing;
		private int cooldownRemaining;

		public StepUpItem(int selectionCount = 3, int? cooldownPasses = null, DiceItemView prefabOverride = null)
			: base("step_up_item", "Step Up", DiceItemActivationType.ClickToActivate)
		{
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

			List<UniTask> animations = null;

			foreach (var dice in diceList)
			{
				if (dice == null)
				{
					continue;
				}

				var nextValue = GetNextValue(dice.CurrentValue);
				dice.SetValue(nextValue);

				if (boundGameModel.ScreenDiceDict != null &&
				    boundGameModel.ScreenDiceDict.TryGetValue(dice, out var view) &&
				    view != null)
				{
					animations ??= new List<UniTask>();
					animations.Add(view.PlayRollAnimationAsync(0.25f));
				}
			}

			if (animations is { Count: > 0 })
			{
				await UniTask.WhenAll(animations);
			}

			UpdatePreview();
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
				if (view == null)
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
				if (kv.Key != null)
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

			if (DiceGameUtils.HasTrashInSelected(values))
			{
				boundGameModel.tableModel.SetPreviewPoints(0);
			}
			else
			{
				var combo = DiceGameUtils.GetCombinations(values);
				boundGameModel.tableModel.SetPreviewPoints(DiceGameUtils.CalculateTotalScore(combo));
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

		public DiceItemView GetViewPrefab() => customPrefab;
	}
}

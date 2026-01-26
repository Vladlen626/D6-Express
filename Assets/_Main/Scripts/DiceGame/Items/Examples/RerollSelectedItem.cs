using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Clickable item: arm it, then the next clicked (unsaved) die rerolls immediately.
	/// After use it goes on cooldown for a set number of Pass actions (default: 2).
	/// </summary>
	public class RerollSelectedItem : DiceItemBase, IOnPassModifier, IOnRoundStartModifier, IDiceItemViewProvider
	{
		private readonly int cooldownLengthInPasses;
		private readonly DiceItemView customPrefab;
		private int cooldownRemaining;
		private DiceGameModel boundGameModel;
		private readonly Dictionary<DiceView, UnityAction> clickHandlers = new();
		private bool handlersAttached;

		public RerollSelectedItem(int cooldownPasses = 2, DiceItemView prefabOverride = null)
			: base("reroll_selected_item", "Second Chance", DiceItemActivationType.ClickToActivate)
		{
			cooldownLengthInPasses = Mathf.Max(1, cooldownPasses);
			customPrefab = prefabOverride;
		}

		public override async UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			TryAttachDiceHandlers(modifierContext.DiceGameModel);

			switch (modifierContext.Stage)
			{
				case ModifierStage.RoundStart:
					// If a new round starts while armed, keep it armed; cooldown is tracked per pass only.
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

			SetState(DiceItemState.Armed);
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
				if (dice == null || dice.IsSaved)
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
			var recomputed = DiceGameUtils.GetCombinations(DiceGameUtils.GetDiceValues(context.Dice));
			var targetList = context.CombinationResult.Combinations;
			targetList.Clear();
			targetList.AddRange(recomputed.Combinations);
		}

		private async void OnDiceClickedAsync(DiceModel model, DiceView view)
		{
			if (State != DiceItemState.Armed || cooldownRemaining > 0 || model == null || model.IsSaved)
			{
				return;
			}

			model.Roll();
			if (view != null)
			{
				await view.PlayRollAnimationAsync(0.35f);
			}

			UpdatePreview();
			BeginCooldown();
		}

		private void TryAttachDiceHandlers(DiceGameModel gameModel)
		{
			if (gameModel == null || gameModel.ScreenDiceDict == null)
			{
				return;
			}

			if (handlersAttached)
			{
				// If this is a new game instance or the dice set changed, reattach.
				if (!ReferenceEquals(boundGameModel, gameModel) ||
				    clickHandlers.Count != gameModel.ScreenDiceDict.Count)
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
				if (view == null)
				{
					continue;
				}

				UnityAction listener = () => OnDiceClickedAsync(model, view);
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
				var total = DiceGameUtils.CalculateTotalScore(combo);
				boundGameModel.tableModel.SetPreviewPoints(total);
			}

			boundGameModel.tableModel.SendUpdateUI();
		}

		private void BeginCooldown()
		{
			cooldownRemaining = cooldownLengthInPasses;
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

		public override void ResetItem()
		{
			base.ResetItem();
			cooldownRemaining = 0;
			DetachDiceHandlers();
		}

		public DiceItemView GetViewPrefab() => customPrefab;
	}
}

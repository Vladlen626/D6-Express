using System.Collections.Generic;
using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Arms on click: select N dice; all selected dice become the median face among selected values.
	/// </summary>
	public class MedianBlendItem : ModifierItemBase, IOnPassModifier, IOnRoundStartModifier, IModifierItemViewProvider
	{
		private readonly DiceScoringService scoringService;
		private readonly int selectionTarget;
		private readonly ItemView customPrefab;
		private readonly HashSet<DiceModel> selectedDice = new();
		private readonly Dictionary<DiceView, UnityAction> clickHandlers = new();
		private DiceGameModel boundGameModel;
		private bool handlersAttached;
		private bool isProcessing;

		public MedianBlendItem(
			string id,
			DiceScoringService scoringService,
			int selectionCount = 3,
			ItemView prefabOverride = null)
			: base(id, id, DiceItemActivationType.ClickToActivate)
		{
			this.scoringService = scoringService;
			selectionTarget = Mathf.Max(3, selectionCount);
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
				_ = ApplyMedianBlendAsync();
			}
		}

		private async UniTask ApplyMedianBlendAsync()
		{
			isProcessing = true;
			try
			{
				var diceList = boundGameModel?.CurrentDiceModelList;
				if (diceList == null || diceList.Count == 0)
				{
					selectedDice.Clear();
					return;
				}

				var availableDice = new HashSet<DiceModel>(diceList);
				var targets = new List<DiceModel>(selectedDice.Count);
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

					targets.Add(dice);
				}

				if (targets.Count == 0)
				{
					selectedDice.Clear();
					return;
				}

				var values = new int[targets.Count];
				for (int i = 0; i < targets.Count; i++)
				{
					values[i] = targets[i].CurrentValue;
				}

				System.Array.Sort(values);
				var median = values[values.Length / 2];

				for (int i = 0; i < targets.Count; i++)
				{
					targets[i].SetValue(median);
				}

				UpdatePreview();
				boundGameModel.NotifyDiceValuesChanged();
				NotifyEffectApplied();
				Consume();
				DetachDiceHandlers();
			}
			finally
			{
				selectedDice.Clear();
				isProcessing = false;
			}

			await UniTask.CompletedTask;
		}

		private void TryAttachDiceHandlers(DiceGameModel gameModel)
		{
			if (gameModel == null || gameModel.ScreenDiceDict == null)
			{
				return;
			}

			if (handlersAttached)
			{
				if (!object.ReferenceEquals(boundGameModel, gameModel) || !HasSameDiceViews(gameModel))
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
			if (boundGameModel?.tableModel == null || scoringService == null)
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

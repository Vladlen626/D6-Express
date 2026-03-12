using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using _Main.Scripts.Core;
using UnityEngine;
using UnityEngine.Events;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Click-to-activate item: select N dice (default 3); after the Nth selection, every die
	/// on the board advances its face by +1 (wrapping 6 -> 1). Then it is consumed.
	/// </summary>
	public class StepUpItem : ModifierItemBase, IOnPassModifier, IOnRoundStartModifier, IModifierItemViewProvider
	{
		private readonly DiceScoringService scoringService;
		private readonly int selectionTarget;
		private readonly ItemView customPrefab;

		private readonly HashSet<DiceModel> selectedDice = new();
		private readonly Dictionary<DiceView, UnityAction> clickHandlers = new();
		private readonly List<GameObject> floatingLabels = new();

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
					// Keep armed state if the player queued the item; just ensure handlers are bound.
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
			NotifyActivationStarted();
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
				ConsumeAndDeactivate();
				return;
			}

			var animateSet = new HashSet<DiceModel>(selectedDice);
			List<UniTask> animations = null;
			List<GameObject> labels = null;

			foreach (var dice in diceList)
			{
				if (dice == null)
				{
					continue;
				}

				var nextValue = GetNextValue(dice.CurrentValue);
				dice.SetValue(nextValue);

				if (animateSet.Contains(dice) &&
				    boundGameModel.ScreenDiceDict != null &&
				    boundGameModel.ScreenDiceDict.TryGetValue(dice, out var view) &&
				    view)
				{
					var label = SpawnLabel(view.transform, "+1");
					if (label)
					{
						labels ??= new List<GameObject>();
						labels.Add(label);
						floatingLabels.Add(label);
					}

					animations ??= new List<UniTask>();
					animations.Add(view.PlayRollAnimationAsync(0.25f));
				}
			}

			if (animations is { Count: > 0 })
			{
				await UniTask.WhenAll(animations);
			}

			if (labels is { Count: > 0 })
			{
				await UniTask.Delay(600);
				foreach (var go in labels)
				{
					if (go)
					{
						floatingLabels.Remove(go);
						Object.Destroy(go);
					}
				}
			}

			UpdatePreview();
			boundGameModel?.NotifyDiceValuesChanged();
			NotifyEffectApplied();
			ConsumeAndDeactivate();
		}

		private static int GetNextValue(int current)
		{
			if (current < 1 || current > 6)
			{
				return 1;
			}

			return current == 6 ? 1 : current + 1;
		}

		private void ConsumeAndDeactivate()
		{
			selectedDice.Clear();
			Consume();
			DetachDiceHandlers();
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
			selectedDice.Clear();
			isProcessing = false;
			DetachDiceHandlers();
			ClearLabels();
		}

		public ItemView GetViewPrefab() => customPrefab;

		private GameObject SpawnLabel(Transform parent, string text)
		{
			if (!parent)
			{
				return null;
			}

			var go = new GameObject("StepUp_Label");
			go.transform.SetParent(parent, false);
			go.transform.localPosition = Vector3.up * 0.4f;

			var tmp = go.AddComponent<TextMeshPro>();
			tmp.text = text;
			tmp.fontSize = 1.2f;
			tmp.enableAutoSizing = true;
			tmp.fontSizeMin = 0.8f;
			tmp.fontSizeMax = 1.6f;
			tmp.color = Color.yellow;
			tmp.alignment = TextAlignmentOptions.Center;
			tmp.sortingOrder = 10;

			return go;
		}

		private void ClearLabels()
		{
			foreach (var go in floatingLabels)
			{
				if (go)
				{
					Object.Destroy(go);
				}
			}

			floatingLabels.Clear();
		}
	}
}

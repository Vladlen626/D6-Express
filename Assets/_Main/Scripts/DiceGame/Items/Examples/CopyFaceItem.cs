using System.Collections.Generic;
using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Arms on click: pick source die, then pick another die that copies source face value.
	/// </summary>
	public class CopyFaceItem : ModifierItemBase, IOnPassModifier, IOnRoundStartModifier, IModifierItemViewProvider
	{
		private readonly DiceScoringService scoringService;
		private readonly ItemView customPrefab;
		private readonly Dictionary<DiceView, UnityAction> clickHandlers = new();
		private DiceGameModel boundGameModel;
		private DiceModel sourceDice;
		private bool handlersAttached;

		public CopyFaceItem(string id, DiceScoringService scoringService, ItemView prefabOverride = null)
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

			sourceDice = null;
			SetState(DiceItemState.Armed);
			NotifyActivationStarted();
			return true;
		}

		private void OnDiceClicked(DiceModel model)
		{
			if (State != DiceItemState.Armed || model == null || boundGameModel == null)
			{
				return;
			}

			if (sourceDice == null)
			{
				sourceDice = model;
				return;
			}

			if (object.ReferenceEquals(sourceDice, model))
			{
				return;
			}

			if (model.IsSaved || DiceGameUtils.IsDiceBanked(model, boundGameModel.tableModel))
			{
				return;
			}

			model.SetValue(sourceDice.CurrentValue);
			UpdatePreview();
			boundGameModel.NotifyDiceValuesChanged();
			NotifyEffectApplied();
			ConsumeAndClear();
		}

		private void ConsumeAndClear()
		{
			sourceDice = null;
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
				if (!object.ReferenceEquals(boundGameModel, gameModel) ||
				    clickHandlers.Count != gameModel.ScreenDiceDict.Count)
				{
					DetachDiceHandlers();
					sourceDice = null;
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

		public override void ResetItem()
		{
			base.ResetItem();
			sourceDice = null;
			DetachDiceHandlers();
		}

		public ItemView GetViewPrefab() => customPrefab;
	}
}

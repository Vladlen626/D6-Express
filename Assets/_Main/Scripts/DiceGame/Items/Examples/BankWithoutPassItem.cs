using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;

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

			if (!BankCurrentPreview())
			{
				SetState(DiceItemState.Ready);
				return false;
			}

			NotifyEffectApplied();
			Consume();
			return true;
		}

		private bool BankCurrentPreview()
		{
			if (boundGameModel == null || scoringService == null)
			{
				return false;
			}

			var table = boundGameModel.tableModel;
			var selected = boundGameModel.GetSelected();
			if (selected == null || selected.Length == 0)
			{
				return false;
			}

			var values = DiceGameUtils.GetDiceValues(selected);
			if (scoringService.HasTrash(values))
			{
				return false;
			}

			var combo = scoringService.Evaluate(values);
			var points = scoringService.CalculateTotalScore(combo);
			if (points <= 0)
			{
				return false;
			}

			table.AddTurnPoints(points);

			for (int i = 0; i < selected.Length; i++)
			{
				var diceModel = selected[i];
				if (diceModel == null)
				{
					continue;
				}

				diceModel.SetSaved(true);
				diceModel.SetChosen(false);

				var position = table.GetFreeBankedPosition();
				if (!position)
				{
					continue;
				}

				diceModel.SetCurrentPosition(position);
				if (!boundGameModel.ScreenDiceDict.TryGetValue(diceModel, out var view) || !view)
				{
					continue;
				}

				view.transform.SetParent(position);
				view.ResetYRotation();
				view.MoveToPosition(position.position);
			}

			boundGameModel.RequestUpgrade(combo);

			if (boundGameModel.AllBanked())
			{
				ResetAllDiceToActive();
			}

			UpdatePreview();
			boundGameModel.NotifyDiceValuesChanged();
			return true;
		}

		private void ResetAllDiceToActive()
		{
			if (boundGameModel?.tableModel == null)
			{
				return;
			}

			var table = boundGameModel.tableModel;
			table.ResetAllPositions();

			var diceList = boundGameModel.CurrentDiceModelList;
			for (int i = 0; i < diceList.Count; i++)
			{
				var diceModel = diceList[i];
				if (diceModel == null)
				{
					continue;
				}

				var position = table.GetFreeActivePosition();
				if (!position)
				{
					continue;
				}

				diceModel.SetSaved(false);
				diceModel.SetChosen(false);
				diceModel.SetCurrentPosition(position);

				if (!boundGameModel.ScreenDiceDict.TryGetValue(diceModel, out var view) || !view)
				{
					continue;
				}

				view.transform.SetParent(position);
				view.MoveToPosition(position.position);
			}

			boundGameModel.ResetAllDices();
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
	}
}

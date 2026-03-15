using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Instant click item: rerolls all unsaved and unbanked dice, then gets consumed.
	/// </summary>
	public class RerollAllUnsavedItem : ModifierItemBase, IGameModelBoundItem, IModifierItemViewProvider
	{
		private readonly DiceScoringService scoringService;
		private readonly ItemView customPrefab;
		private DiceGameModel boundGameModel;

		public RerollAllUnsavedItem(string id, DiceScoringService scoringService, ItemView prefabOverride = null)
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
			if (State != DiceItemState.Ready || boundGameModel == null)
			{
				return false;
			}

			SetState(DiceItemState.Armed);
			NotifyActivationStarted();

			if (!ApplyReroll())
			{
				SetState(DiceItemState.Ready);
				return false;
			}

			NotifyEffectApplied();
			Consume();
			return true;
		}

		private bool ApplyReroll()
		{
			if (boundGameModel == null)
			{
				return false;
			}

			var unbanked = boundGameModel.GetUnbanked();
			if (unbanked == null || unbanked.Length == 0)
			{
				return false;
			}

			var rerolled = false;
			for (int i = 0; i < unbanked.Length; i++)
			{
				var dice = unbanked[i];
				if (dice == null || dice.IsSaved || DiceGameUtils.IsDiceBanked(dice, boundGameModel.tableModel))
				{
					continue;
				}

				dice.Roll();
				rerolled = true;
			}

			if (!rerolled)
			{
				return false;
			}

			UpdatePreview();
			boundGameModel.NotifyDiceValuesChanged();
			return true;
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

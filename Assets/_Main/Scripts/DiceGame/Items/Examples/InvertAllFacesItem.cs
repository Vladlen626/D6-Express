using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Instant click item: flips all current dice to opposite faces and gets consumed.
	/// </summary>
	public class InvertAllFacesItem : ModifierItemBase, IGameModelBoundItem, IModifierItemViewProvider
	{
		private readonly DiceScoringService scoringService;
		private readonly ItemView customPrefab;
		private DiceGameModel boundGameModel;

		public InvertAllFacesItem(string id, DiceScoringService scoringService, ItemView prefabOverride = null)
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

			if (!ApplyInvert())
			{
				SetState(DiceItemState.Ready);
				return false;
			}

			NotifyEffectApplied();
			Consume();
			return true;
		}

		private bool ApplyInvert()
		{
			var diceList = boundGameModel?.CurrentDiceModelList;
			if (diceList == null || diceList.Count == 0)
			{
				return false;
			}

			var changed = false;
			for (int i = 0; i < diceList.Count; i++)
			{
				var dice = diceList[i];
				if (dice == null)
				{
					continue;
				}

				var flipped = GetOppositeValue(dice.CurrentValue);
				dice.SetValue(flipped);
				changed = true;
			}

			if (!changed)
			{
				return false;
			}

			UpdatePreview();
			boundGameModel.NotifyDiceValuesChanged();
			return true;
		}

		private static int GetOppositeValue(int currentValue)
		{
			switch (currentValue)
			{
				case 1:
					return 6;
				case 2:
					return 5;
				case 3:
					return 4;
				case 4:
					return 3;
				case 5:
					return 2;
				case 6:
					return 1;
				default:
					return 1;
			}
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

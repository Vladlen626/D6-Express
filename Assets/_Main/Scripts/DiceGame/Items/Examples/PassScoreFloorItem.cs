using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Arms on click: next pass score cannot be lower than pocket cash amount captured on activation.
	/// </summary>
	public class PassScoreFloorItem : ModifierItemBase, IOnPassModifier, IModifierItemViewProvider
	{
		private readonly DiceScoringService scoringService;
		private readonly InventoryModel inventoryModel;
		private readonly ItemView customPrefab;
		private int cachedCashFloor;

		public PassScoreFloorItem(
			string id,
			DiceScoringService scoringService,
			InventoryModel inventoryModel,
			ItemView prefabOverride = null)
			: base(id, id, DiceItemActivationType.ClickToActivate)
		{
			this.scoringService = scoringService;
			this.inventoryModel = inventoryModel;
			customPrefab = prefabOverride;
		}

		public override string InvalidActivationNotificationKey => GlobalConstants.Localization.ItemActivationOnlyGame;

		public override bool IsActivationAllowed(DiceGameState gameState)
		{
			return gameState == DiceGameState.GAME;
		}

		protected override bool OnClick()
		{
			if (State != DiceItemState.Ready || inventoryModel == null)
			{
				return false;
			}

			cachedCashFloor = inventoryModel.CashCount;
			SetState(DiceItemState.Armed);
			NotifyActivationStarted();
			return true;
		}

		public override UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			if (modifierContext.Stage != ModifierStage.Pass || State != DiceItemState.Armed || scoringService == null)
			{
				return UniTask.CompletedTask;
			}

			var result = modifierContext.CombinationResult;
			var currentTotal = scoringService.CalculateTotalScore(result);
			if (currentTotal < cachedCashFloor)
			{
				var delta = cachedCashFloor - currentTotal;
				var combinations = result.Combinations;
				if (combinations == null)
				{
					cachedCashFloor = 0;
					Consume();
					return UniTask.CompletedTask;
				}

				if (combinations.Count == 0)
				{
					combinations.Add(new DiceCombinationEntry
					{
						Id = "pass_score_floor_bonus",
						DisplayName = "pass_score_floor_bonus",
						Combination = DiceCombination.None,
						Face = 0,
						Count = 0,
						BaseScore = delta,
						Multiplier = 1
					});
				}
				else
				{
					combinations[0].BaseScore += delta;
				}

				NotifyEffectApplied();
			}

			cachedCashFloor = 0;
			Consume();
			return UniTask.CompletedTask;
		}

		public override void ResetItem()
		{
			base.ResetItem();
			cachedCashFloor = 0;
		}

		public ItemView GetViewPrefab() => customPrefab;
	}
}

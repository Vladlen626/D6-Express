using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Passive item that increases the maximum dice cap by a fixed amount (default +4).
	/// The mechanic is reusable by other items/modifiers via DiceGameModel.SetDiceCapModifier.
	/// </summary>
	public class ExtraDiceCapItem : DiceItemBase, IOnLevelStartModifier, IGameModelBoundItem, IDiceItemViewProvider
	{
		private readonly int bonus;
		private readonly DiceItemView customPrefab;
		private DiceGameModel boundGameModel;
		private const string BonusKey = "extra_dice_cap_item";

		public ExtraDiceCapItem(int bonus = 4, DiceItemView prefabOverride = null)
			: base("extra_dice_cap", "Extra Dice", DiceItemActivationType.Passive)
		{
			this.bonus = Mathf.Max(1, bonus);
			customPrefab = prefabOverride;
		}

		public override async UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			// Ensure the bonus is applied at level start so selections/setup use the expanded cap.
			if (modifierContext.Stage == ModifierStage.LevelStart)
			{
				ApplyBonus(modifierContext.DiceGameModel);
			}

			await UniTask.CompletedTask;
		}

		public void OnAddedToGameModel(DiceGameModel gameModel)
		{
			ApplyBonus(gameModel);
		}

		public void OnRemovedFromGameModel(DiceGameModel gameModel)
		{
			gameModel?.RemoveDiceCapModifier(BonusKey);
			boundGameModel = null;
		}

		public override void ResetItem()
		{
			base.ResetItem();
			boundGameModel?.RemoveDiceCapModifier(BonusKey);
			boundGameModel = null;
		}

		public DiceItemView GetViewPrefab() => customPrefab;

		private void ApplyBonus(DiceGameModel gameModel)
		{
			if (gameModel == null)
			{
				return;
			}

			boundGameModel = gameModel;
			boundGameModel.SetDiceCapModifier(BonusKey, bonus);
		}
	}
}

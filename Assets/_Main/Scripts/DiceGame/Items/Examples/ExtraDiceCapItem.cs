using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Passive item that increases the maximum dice cap by a fixed amount (default +4).
	/// The mechanic is reusable by other items/modifiers via DiceGameModel.SetDiceCapModifier.
	/// </summary>
	public class ExtraDiceCapItem : ModifierItemBase, IOnLevelStartModifier, IGameModelBoundItem, IModifierItemViewProvider
	{
		private readonly int bonus;
		private readonly DiceItemView customPrefab;
		private DiceGameModel boundGameModel;
		private readonly string bonusKey;

		public ExtraDiceCapItem(string id, int bonus = 4, DiceItemView prefabOverride = null)
			: base(id, id, DiceItemActivationType.Passive)
		{
			this.bonus = Mathf.Max(1, bonus);
			customPrefab = prefabOverride;
			bonusKey = id;
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
			gameModel?.RemoveDiceCapModifier(bonusKey);
			boundGameModel = null;
		}

		public override void ResetItem()
		{
			base.ResetItem();
			boundGameModel?.RemoveDiceCapModifier(bonusKey);
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
			boundGameModel.SetDiceCapModifier(bonusKey, bonus);
		}
	}
}

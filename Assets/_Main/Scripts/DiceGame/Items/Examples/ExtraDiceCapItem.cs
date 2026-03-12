using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Click-to-activate item that increases the maximum dice cap by a fixed amount (default +4)
	/// only for the current match. Activation is allowed during selection stage only.
	/// </summary>
	public class ExtraDiceCapItem : ModifierItemBase, IGameModelBoundItem, IModifierItemViewProvider, IOnMatchFinishedItem
	{
		private readonly int bonus;
		private readonly DiceItemView customPrefab;
		private DiceGameModel boundGameModel;
		private readonly string bonusKey;
		private bool boundIsPlayerSide = true;

		public ExtraDiceCapItem(string id, int bonus = 4, DiceItemView prefabOverride = null)
			: base(id, id, DiceItemActivationType.ClickToActivate)
		{
			this.bonus = Mathf.Max(1, bonus);
			customPrefab = prefabOverride;
			bonusKey = id;
		}

		protected override bool OnClick()
		{
			if (State != DiceItemState.Ready)
			{
				return false;
			}

			if (boundGameModel == null || boundGameModel.DiceGameState != DiceGameState.SELECT_DICE)
			{
				return false;
			}

			ApplyBonus();
			SetState(DiceItemState.Armed);
			return true;
		}

		public override async UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			await UniTask.CompletedTask;
		}

		public void OnAddedToGameModel(DiceGameModel gameModel)
		{
			BindGameModel(gameModel);
		}

		public void OnRemovedFromGameModel(DiceGameModel gameModel)
		{
			RemoveBonus();
			boundGameModel = null;
		}

		public void OnMatchFinished()
		{
			if (State != DiceItemState.Armed)
			{
				return;
			}

			RemoveBonus();
			Consume();
		}

		public override void ResetItem()
		{
			RemoveBonus();
			base.ResetItem();
			boundGameModel = null;
		}

		public DiceItemView GetViewPrefab() => customPrefab;

		private void BindGameModel(DiceGameModel gameModel)
		{
			if (gameModel == null)
			{
				return;
			}

			boundGameModel = gameModel;
			boundIsPlayerSide = ResolveBoundSide(gameModel);
		}

		private void ApplyBonus()
		{
			if (boundGameModel == null)
			{
				return;
			}

			boundGameModel.SetDiceCapModifier(bonusKey, bonus, boundIsPlayerSide);
		}

		private void RemoveBonus()
		{
			boundGameModel?.RemoveDiceCapModifier(bonusKey, boundIsPlayerSide);
		}

		private bool ResolveBoundSide(DiceGameModel gameModel)
		{
			if (gameModel.PlayerModifierItemsModel.Items.Contains(this))
			{
				return true;
			}

			if (gameModel.EnemyModifierItemsModel.Items.Contains(this))
			{
				return false;
			}

			return gameModel.IsPlayerTurn;
		}
	}
}

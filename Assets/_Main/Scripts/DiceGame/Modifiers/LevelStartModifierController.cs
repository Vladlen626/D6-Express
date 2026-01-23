using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Bridges level start events to dice modifiers that implement IOnLevelStartModifier.
	/// </summary>
	public class LevelStartModifierController : IBaseController, IActivatable
	{
		private readonly LevelModel levelModel;
		private readonly DiceGameModel diceGameModel;

		public LevelStartModifierController(LevelModel levelModel, DiceGameModel diceGameModel)
		{
			this.levelModel = levelModel;
			this.diceGameModel = diceGameModel;
		}

		public void Activate()
		{
			levelModel.LevelStarted += OnLevelStarted;
		}

		public void Deactivate()
		{
			levelModel.LevelStarted -= OnLevelStarted;
		}

		private void OnLevelStarted()
		{
			var context = new DiceModifierContext(
				new DiceCombinationResult { Combinations = new List<DiceCombinationEntry>() },
				System.Array.Empty<DiceModel>(),
				null,
				diceGameModel,
				ModifierStage.LevelStart,
				levelModel);

			// Fire and forget; LevelStart modifiers are expected to be quick.
			diceGameModel.ModifiersModel.PlayLevelStartActions(context).Forget();
		}
	}
}

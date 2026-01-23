using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Bridges level start events to dice modifiers that implement IOnLevelStartModifier.
	/// </summary>
	public class LevelStartModifierController : IBaseController, IActivatable
	{
		private readonly Run run;
		private readonly DiceGameModel diceGameModel;

		public LevelStartModifierController(Run run, DiceGameModel diceGameModel)
		{
			this.run = run;
			this.diceGameModel = diceGameModel;
		}

		public void Activate()
		{
			run.ProgressChanged += OnProgressChanged;
		}

		public void Deactivate()
		{
			run.ProgressChanged -= OnProgressChanged;
		}

		private void OnProgressChanged(Run.ProgressType progressType)
		{
			if (progressType == Run.ProgressType.STARTED)
			{
				OnLevelStarted();
			}
		}

		private void OnLevelStarted()
		{
			var context = new DiceModifierContext(
				new DiceCombinationResult { Combinations = new List<DiceCombinationEntry>() },
				System.Array.Empty<DiceModel>(),
				null,
				diceGameModel,
				ModifierStage.LevelStart,
				run);

			// Fire and forget; LevelStart modifiers are expected to be quick.
			diceGameModel.ModifiersModel.PlayLevelStartActions(context).Forget();
		}
	}
}

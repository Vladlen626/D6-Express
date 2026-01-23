using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Adjusts the number of ticks (games per day) for the current level by a fixed delta (can increase or decrease).
	/// Applied on RoundStart; persists for the level until LevelModel.UpdateLevel is called for the next stage.
	/// </summary>
	public class AdjustTicksPerDayModifier : IOnLevelStartModifier
	{
		private readonly int delta;
		private bool isApplied;

		/// <param name="delta">Positive to increase ticks per day, negative to reduce. Defaults to -1 (reduce by one).</param>
		public AdjustTicksPerDayModifier(int delta = -1)
		{
			this.delta = delta;
		}

		public UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			if (modifierContext.Run == null)
			{
				return UniTask.CompletedTask;
			}

			if (modifierContext.Stage == ModifierStage.LevelStart)
			{
				Apply(modifierContext.Run);
			}

			return UniTask.CompletedTask;
		}

		private void Apply(Run run)
		{
			if (isApplied)
			{
				return;
			}

			var newTicks = run.TicksPerDay + delta;
			if (newTicks < 1)
			{
				newTicks = 1;
			}

			run.SetTicksPerDay(newTicks);
			isApplied = true;
		}
	}
}

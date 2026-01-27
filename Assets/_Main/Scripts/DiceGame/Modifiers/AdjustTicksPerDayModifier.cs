using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Adjusts the number of ticks (games per day) for the current level by a fixed delta (can increase or decrease).
	/// Applied on LevelStart; reverted at the end of the level by subtracting the same delta from the current value.
	/// </summary>
	public class AdjustTicksPerDayModifier : IOnLevelStartModifier
	{
		private readonly int delta;
		private bool isApplied;
		private Run run;

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

			this.run = run;

			var newTicks = run.TicksPerDay + delta;
			if (newTicks < 1)
			{
				newTicks = 1;
			}

			run.SetTicksPerDay(newTicks);
			run.LevelChanged += OnLevelEnded;
			run.RunFinished += OnRunFinished;
			isApplied = true;
		}

		private void OnLevelEnded()
		{
			Reset();
		}

		private void OnRunFinished(bool result)
		{
			Reset();
		}

		private void Reset()
		{
			if (!isApplied || run == null)
			{
				return;
			}

			var restoredTicks = run.TicksPerDay - delta;
			if (restoredTicks < 1)
			{
				restoredTicks = 1;
			}

			run.SetTicksPerDay(restoredTicks);
			run.LevelChanged -= OnLevelEnded;
			run.RunFinished -= OnRunFinished;

			run = null;
			isApplied = false;
		}
	}
}

using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Reduces the number of ticks (games per day) for the current level by a fixed amount.
	/// Applied on RoundStart; persists for the level until LevelModel.UpdateLevel is called for the next stage.
	/// </summary>
	public class ReduceTicksPerDayModifier : IOnRoundStartModifier
	{
		private readonly int reduction;
		private bool isApplied;

		public ReduceTicksPerDayModifier(int reduction = 1)
		{
			this.reduction = reduction;
		}

		public UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			if (modifierContext.LevelModel == null)
			{
				return UniTask.CompletedTask;
			}

			if (modifierContext.Stage == ModifierStage.RoundStart)
			{
				Apply(modifierContext.LevelModel);
			}

			return UniTask.CompletedTask;
		}

		private void Apply(LevelModel levelModel)
		{
			if (isApplied)
			{
				return;
			}

			var newTicks = levelModel.Ticks - reduction;
			if (newTicks < 1)
			{
				newTicks = 1;
			}

			SetTicks(levelModel, newTicks);
			isApplied = true;
		}

		/// <summary>
		/// LevelModel.Ticks has a private setter; reflection keeps the change localized to this modifier.
		/// </summary>
		private static void SetTicks(LevelModel levelModel, int value)
		{
			var property = typeof(LevelModel).GetProperty("Ticks");
			property?.SetValue(levelModel, value);
		}
	}
}

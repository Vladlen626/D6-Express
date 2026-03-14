namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Backward-compatible wrapper over AdjustRunScalarModifier for ticks-per-day adjustments.
	/// </summary>
	public class AdjustTicksPerDayModifier : AdjustRunScalarModifier
	{
		public AdjustTicksPerDayModifier(int delta = -1, string uiConfigId = null)
			: base(RunScalarTarget.TicksPerDay, delta, revertOnLevelOrRunEnd: true, uiConfigId)
		{
		}
	}
}

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Backward-compatible wrapper over RerollUnsavedDieModifier.
	/// </summary>
	public class ShakeRerollModifier : RerollUnsavedDieModifier
	{
		public readonly float shakeChance;

		public ShakeRerollModifier(
			DiceScoringService scoringService,
			float shakeChance = 0.95f,
			float rerollAnimationDuration = 0.5f,
			string uiConfigId = null)
			: base(
				scoringService,
				UnsavedDieSelectionStrategy.Random,
				shakeChance,
				rerollAnimationDuration,
				uiConfigId)
		{
			this.shakeChance = UnityEngine.Mathf.Clamp01(shakeChance);
		}
	}
}

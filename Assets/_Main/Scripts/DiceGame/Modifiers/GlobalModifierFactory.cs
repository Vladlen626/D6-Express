namespace _Main.Scripts.Dice
{
	public static class GlobalModifierFactory
	{
		public static IModifier Create(string modifierId, DiceScoringService scoringService)
		{
			if (string.IsNullOrWhiteSpace(modifierId))
			{
				return null;
			}

			switch (modifierId)
			{
				case "multiply_combo_three_kind":
					return new MultiplyComboModifier(DiceCombination.ThreeOfAKind);

				case "multiply_kind_single_ones":
					return new MultiplyKindOfModifiers(DiceCombination.SingleOnes, 1);

				case "shake_reroll":
					if (scoringService == null)
					{
						return null;
					}
					return new ShakeRerollModifier(scoringService, 0.95f, 0.5f);

				case "scramble_combinations":
					if (scoringService == null)
					{
						return null;
					}
					return new ScrambleCombinationsModifier(scoringService);

				case "pass_activation_multiplier":
					return new PassActivationMultiplierModifier();

				case "adjust_ticks_plus1":
					return new AdjustTicksPerDayModifier(1);
			}

			return null;
		}
	}
}

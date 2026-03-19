namespace _Main.Scripts.Dice
{
	public enum DiceMatchResultReason
	{
		Unknown = 0,
		PlayerReachedTarget,
		EnemyReachedTarget,
		SetupFailed,
		EnemyAiValidationFailed,
		EnemyAiException,
		DebugForced
	}

	public enum DiceMatchStage
	{
		Unknown = 0,
		Setup,
		SelectDice,
		Bet,
		Roll,
		Pass,
		RoundEnd,
		EnemyTurn
	}
}

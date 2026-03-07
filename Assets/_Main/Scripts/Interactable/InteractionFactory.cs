public static class InteractionFactory
{
	public static InteractionToStateTable CreateTable()
	{
		var table = new InteractionToStateTable();

		table.SetAllowance(InteractionType.SIT, CharacterState.DEFAULT, true);

		table.SetAllowance(InteractionType.LAY, CharacterState.DEFAULT, true);

		table.SetAllowance(InteractionType.PLAY_DICE, CharacterState.DEFAULT, true);

		table.SetAllowance(InteractionType.SLEEP, CharacterState.LAYING, true);

		table.SetAllowance(InteractionType.TRADE, CharacterState.DEFAULT, true);

		table.SetAllowance(InteractionType.SPEAK, CharacterState.DEFAULT, true);
		table.SetAllowance(InteractionType.SPEAK, CharacterState.SITTING, true);
		table.SetAllowance(InteractionType.SPEAK, CharacterState.LAYING, true);
		table.SetAllowance(InteractionType.SPEAK, CharacterState.DICE_GAME, true);

		table.SetAllowance(InteractionType.RESTOCK, CharacterState.DEFAULT, true);

		table.SetAllowance(InteractionType.OPEN, CharacterState.DEFAULT, true);

		table.SetAllowance(InteractionType.CLOSE, CharacterState.DEFAULT, true);

		table.SetAllowance(InteractionType.FART, CharacterState.DEFAULT, true);
		table.SetAllowance(InteractionType.FART, CharacterState.SITTING, true);
		table.SetAllowance(InteractionType.FART, CharacterState.LAYING, true);

		return table;
	}
}
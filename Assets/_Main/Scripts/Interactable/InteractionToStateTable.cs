using System;
using System.Collections.Generic;

public class InteractionToStateTable
{
	private readonly bool[,] table = new bool[Enum.GetValues(typeof(InteractionType)).Length, Enum.GetValues(typeof(CharacterState)).Length];

	public void SetAllowance(InteractionType type, CharacterState state, bool value)
	{
		table[(int)type, (int)state] = value;
	}

	public bool IsAllowed(InteractionType type, CharacterState state)
	{
		return table[(int)type, (int)state];
	}

	public bool IsAllowed(InteractionType type, IEnumerable<CharacterState> states)
	{
		foreach (var state in states)
		{
			if (!table[(int)type, (int)state])
			{
				return false;
			}
		}

		return true;
	}
}
using System;
using System.Collections.Generic;

public class PlayerStateModel
{
	private readonly List<CharacterState> currentStates = new();
	private readonly Dictionary<CharacterState, CharacterStateHandler> dictStates = new();

	public event Action StatesChanged;
	public event Action<CharacterState> StateAdded;
	public event Action<CharacterState> StateRemoved;

	public IReadOnlyList<CharacterState> CurrentStates => currentStates.AsReadOnly();

	public void FillCharacterStatesDict(CharacterStateHandler[] stateHandlers)
	{
		foreach (var characterStateHandler in stateHandlers)
		{
			dictStates[characterStateHandler.State] = characterStateHandler;
		}
	}
	
	public bool HasState(CharacterState state)
	{
		return currentStates.Contains(state);
	}

	public void TryAddState(CharacterState state)
	{
		if (currentStates.Contains(state))
			return;

		if (dictStates.ContainsKey(state))
		{
			var stateHandler = dictStates[state];
			stateHandler.Enter();
		}

		currentStates.Add(state);

		StateAdded?.Invoke(state);
		StatesChanged?.Invoke();
	}

	public void TryRemoveState(CharacterState state)
	{
		if (dictStates.ContainsKey(state))
		{
			var stateHandler = dictStates[state];
			stateHandler.Exit();
		}

		currentStates.Remove(state);

		StateRemoved?.Invoke(state);
		StatesChanged?.Invoke();
	}
}
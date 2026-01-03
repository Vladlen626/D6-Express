using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateController : MonoBehaviour
{
    [SerializeReference]
    [SubclassSelector]
    private List<CharacterStateHandler> states = new();

    private readonly List<CharacterState> currentStates = new();
    private readonly Dictionary<CharacterState, CharacterStateHandler> dictStates = new();

    public event Action StatesChanged;
    public event Action<CharacterState> StateAdded;
    public event Action<CharacterState> StateRemoved;

    public IReadOnlyList<CharacterState> CurrentStates => currentStates.AsReadOnly();

    public bool HasState(CharacterState state)
    {
        return currentStates.Contains(state);
    }

    public void Initialize()
    {
        foreach (var item in states)
        {
            item.Init(this);
            dictStates[item.State] = item;
        }

        TryAddState(CharacterState.DEFAULT);
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

using System;
using System.Collections.Generic;
using PlatformCore.Core;
using PlatformCore.Services;
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

        currentStates.Add(state);
        Locator.Resolve<ILoggerService>().Log($"state added: {state}");

        if (dictStates.ContainsKey(state))
        {
            var stateHandler = dictStates[state];

            stateHandler.Exited += RemoveState;

            stateHandler.Enter();
        }

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
        else
        {
            RemoveState(state);
        }
    }

    private void RemoveState(CharacterState state)
    {
        if (dictStates.ContainsKey(state))
        {
            var stateHandler = dictStates[state];
            stateHandler.Exited -= RemoveState;
        }

        currentStates.Remove(state);
        Locator.Resolve<ILoggerService>().Log($"state removed: {state}");

        StateRemoved?.Invoke(state);
        StatesChanged?.Invoke();
    }
}

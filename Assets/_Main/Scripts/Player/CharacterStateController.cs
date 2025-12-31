using System.Collections.Generic;
using UnityEngine;

public class CharacterStateController : MonoBehaviour
{
    [SerializeReference]
    [SubclassSelector]
    private List<CharacterStateHandler> states = new();

    private readonly Dictionary<CharacterState, CharacterStateHandler> dictStates = new();

    public CharacterState State { get; private set; }

    private void Awake()
    {
        foreach (var item in states)
        {
            item.Init(this);
            dictStates[item.State] = item;
        }

        EnterState(CharacterState.DEFAULT);
    }

    private void Start()
    {
        foreach (var item in states)
        {
            item.Start();
        }
    }

    public void TryEnterState(CharacterState state)
    {
        EnterState(state);
    }

    private void EnterState(CharacterState state)
    {
        if (dictStates.ContainsKey(State))
        {
            dictStates[State].Exit();
        }

        if (dictStates.ContainsKey(state))
        {
            dictStates[state].Enter();
        }

        State = state;
    }
}

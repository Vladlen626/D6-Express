using System.Collections.Generic;
using UnityEngine;

public class CharacterStateController : MonoBehaviour
{
    [SerializeReference]
    [SubclassSelector]
    private List<CharacterStateHandler> states = new();

    private readonly Dictionary<CharacterState, CharacterStateHandler> dictStates = new();

    private PlayerModel playerModel;
    public CharacterState State => playerModel.currentCharacterState;

    public void Initialize(PlayerModel playerModel)
    {
        this.playerModel = playerModel;
        
        foreach (var item in states)
        {
            item.Init(this);
            dictStates[item.State] = item;
        }

        EnterState(CharacterState.DEFAULT);
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
        playerModel.SetCharacterState(state);
    }
}

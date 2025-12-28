using System;
using UnityEngine;

[Serializable]
public class CharacterStateHandlerLaying : CharacterStateHandler
{
    public override CharacterState State => CharacterState.LAYING;

    public override void Enter()
    {
        Controller.GetComponent<CharacterController>().enabled = false;
        Controller.GetComponent<Collider>().enabled = false;
    }

    public override void Exit()
    {
        Controller.GetComponent<CharacterController>().enabled = true;
        Controller.GetComponent<Collider>().enabled = true;
    }
}
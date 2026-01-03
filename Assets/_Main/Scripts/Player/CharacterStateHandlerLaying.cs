using System;
using UnityEngine;

[Serializable]
public class CharacterStateHandlerLaying : CharacterStateHandler
{
    public override CharacterState State => CharacterState.LAYING;

    protected override void EnterInternal()
    {
        Controller.GetComponent<CharacterController>().enabled = false;
        Controller.GetComponent<Collider>().enabled = false;
    }

    protected override void ExitInternal()
    {
        Controller.GetComponent<CharacterController>().enabled = true;
        Controller.GetComponent<Collider>().enabled = true;
    }
}
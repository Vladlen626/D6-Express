using System;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using UnityEngine;

[Serializable]
public class CharacterStateHandlerSleeping : CharacterStateHandler
{
    private IInputService inputService;

    public override CharacterState State => CharacterState.SLEEPING;

    public override void OnInit()
    {
        inputService = Locator.Resolve<IInputService>();
    }

    protected override void EnterInternal()
    {
        Controller.GetComponent<CharacterController>().enabled = false;
        Controller.GetComponent<Collider>().enabled = false;
        inputService.DisableAllInputs();
    }

    protected override void ExitInternal()
    {
        Controller.GetComponent<CharacterController>().enabled = true;
        Controller.GetComponent<Collider>().enabled = true;
        inputService.EnableAllInputs();
    }
}
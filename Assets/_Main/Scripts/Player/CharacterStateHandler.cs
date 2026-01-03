using System;

[Serializable]
public abstract class CharacterStateHandler
{
    public abstract CharacterState State { get; }

    protected CharacterStateController Controller { get; private set; }

    public event Action<CharacterState> Entered;
    public event Action<CharacterState> Exited;

    public void Init(CharacterStateController characterStateController)
    {
        Controller = characterStateController;
        OnInit();
    }

    public void Enter()
    {
        EnterInternal();

        Entered?.Invoke(State);
    }

    public void Exit()
    {
        ExitInternal();

        Exited?.Invoke(State);
    }

    protected virtual void EnterInternal() { }
    protected virtual void ExitInternal() { }

    public virtual void OnInit() { }
}
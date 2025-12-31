using System;

[Serializable]
public abstract class CharacterStateHandler
{
    public abstract CharacterState State { get; }

    protected CharacterStateController Controller { get; private set; }

    public void Init(CharacterStateController characterStateController)
    {
        Controller = characterStateController;
    }

    public abstract void Enter();
    public abstract void Exit();

    public virtual void Start() { }
}
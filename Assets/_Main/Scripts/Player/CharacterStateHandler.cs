using System;

[Serializable]
public abstract class CharacterStateHandler
{
	public abstract CharacterState State { get; }

	protected PlayerView PlayerView { get; private set; }

	public void Init(PlayerView playerView)
	{
		PlayerView = playerView;
		OnInit();
	}

	public virtual void Enter()
	{
		PlayerView.SetCharacterGhost(true);
	}
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

	public virtual void Exit()
	{
		PlayerView.SetCharacterGhost(false);
	}

	public virtual void OnInit()
	{
	}
}
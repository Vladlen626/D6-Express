using System;

[Serializable]
public abstract class CharacterStateHandler
{
	public event Action<CharacterState> Entered;
	public event Action<CharacterState> Exited;
	public abstract CharacterState State { get; }

	protected PlayerView PlayerView { get; private set; }

	public void Init(PlayerView playerView)
	{
		PlayerView = playerView;
		OnInit();
	}
	
	public virtual void OnInit()
	{
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

	protected virtual void EnterInternal()
	{
		PlayerView.SetCharacterGhost(true);
	}

	protected virtual void ExitInternal()
	{
		PlayerView.SetCharacterGhost(false);
	}
}
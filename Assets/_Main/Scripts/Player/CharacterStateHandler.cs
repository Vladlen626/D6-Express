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

	public virtual void Exit()
	{
		PlayerView.SetCharacterGhost(false);
	}

	public virtual void OnInit()
	{
	}
}
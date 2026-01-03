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
		PlayerView.SetCharacterControllerEnabled(false);
		PlayerView.SetColliderEnabled(false);
	}

	public virtual void Exit()
	{
		PlayerView.SetCharacterControllerEnabled(true);
		PlayerView.SetColliderEnabled(true);
	}

	public virtual void OnInit()
	{
	}
}
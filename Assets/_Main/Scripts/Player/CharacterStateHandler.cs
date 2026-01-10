using System;
using UnityEngine;

[Serializable]
public abstract class CharacterStateHandler
{
	[SerializeField]
	private bool switchGhost = true;

	public event Action<CharacterState> Entered;
	public event Action<CharacterState> Exited;
	public abstract CharacterState State { get; }

	protected CharacterView CharacterView { get; private set; }

	public void Init(CharacterView characterView)
	{
		CharacterView = characterView;
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
		if (switchGhost)
		{
			CharacterView.SetCharacterGhost(true);
		}
	}

	protected virtual void ExitInternal()
	{
		if (switchGhost)
		{
			CharacterView.SetCharacterGhost(false);
		}
	}
}
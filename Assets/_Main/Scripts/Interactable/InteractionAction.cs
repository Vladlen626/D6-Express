using System;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;

[Serializable]
public abstract class InteractionAction
{
	protected IInputService inputService;
	protected Interactor Interactor { get; private set; }
	protected PlayerStateModel PlayerStateModel { get; private set; }

	public event Action Started;
	public event Action Ended;

	public virtual void Init(Interactor interactor, PlayerStateModel playerStateModel, IInputService inputService)
	{
		Interactor = interactor;
		PlayerStateModel = playerStateModel;
		this.inputService = inputService;
	}

	public abstract bool CanInteract(IInteractable interactable);

	public void StartInteract(IInteractable interactable)
	{
		StartInteractInternal(interactable);

		Started?.Invoke();
	}

	public void StopInteract(IInteractable interactable)
	{
		StopInteractInternal(interactable);

		Ended?.Invoke();
	}

	protected virtual void StartInteractInternal(IInteractable interactable) { }
	protected virtual void StopInteractInternal(IInteractable interactable) { }
}
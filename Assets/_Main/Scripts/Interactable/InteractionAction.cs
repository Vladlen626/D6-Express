using System;

[Serializable]
public abstract class InteractionAction
{
	protected Interactor Interactor { get; private set; }

	public void Init(Interactor interactor)
	{
		Interactor = interactor;
	}

	public abstract bool CanInteract(IInteractable interactable);
	public abstract void StartInteract(IInteractable interactable);
	public abstract void StopInteract(IInteractable interactable);
}
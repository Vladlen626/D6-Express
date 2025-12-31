using System;

[Serializable]
public abstract class InteractionAction
{
	public bool InAction { get; private set; }
	protected Interactor Interactor { get; private set; }

	public event Action Started;
	public event Action Ended;

	public virtual void Init(Interactor interactor)
	{
		Interactor = interactor;
	}

	public abstract bool CanInteract(IInteractable interactable);

	public void StartInteract(IInteractable interactable)
	{
		InAction = true;
		Started?.Invoke();

		StartInteractInternal(interactable);
	}

	public void StopInteract(IInteractable interactable)
	{
		InAction = false;
		StopInteractInternal(interactable);

		Ended?.Invoke();
	}

	protected virtual void StartInteractInternal(IInteractable interactable) { }
	protected virtual void StopInteractInternal(IInteractable interactable) { }
}
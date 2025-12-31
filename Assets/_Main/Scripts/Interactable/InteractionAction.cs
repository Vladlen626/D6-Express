using System;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;

[Serializable]
public abstract class InteractionAction
{
	protected IInputService inputService;
	protected Interactor Interactor { get; private set; }

	public event Action Started;
	public event Action Ended;

	public virtual void Init(Interactor interactor)
	{
		Interactor = interactor;
	}

	public virtual void Start()
	{
		inputService = Locator.Resolve<IInputService>();
	}

	public abstract bool CanInteract(IInteractable interactable);

	public void StartInteract(IInteractable interactable)
	{
		Started?.Invoke();

		StartInteractInternal(interactable);
	}

	public void StopInteract(IInteractable interactable)
	{
		StopInteractInternal(interactable);

		Ended?.Invoke();
	}

	protected virtual void StartInteractInternal(IInteractable interactable) { }
	protected virtual void StopInteractInternal(IInteractable interactable) { }
}
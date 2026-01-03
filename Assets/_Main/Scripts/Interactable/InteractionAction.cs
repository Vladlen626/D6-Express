using System;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;

[Serializable]
public abstract class InteractionAction
{
	protected IInputService inputService;
	protected Interactor Interactor { get; private set; }

	public event Action<InteractionAction> Started;
	public event Action<InteractionAction> Ended;

	public virtual void Init(Interactor interactor)
	{
		Interactor = interactor;
	}

	public void Start()
	{
		inputService = Locator.Resolve<IInputService>();
	}

	public abstract bool CanInteract(IInteractable interactable);

	public void StartInteract(IInteractable interactable)
	{
		StartInteractInternal(interactable);

		Started?.Invoke(this);
	}

	public void StopInteract(IInteractable interactable)
	{
		StopInteractInternal(interactable);

		Ended?.Invoke(this);
	}

	protected virtual void StartInteractInternal(IInteractable interactable) { }
	protected virtual void StopInteractInternal(IInteractable interactable) { }
}
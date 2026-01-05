using System;
using _Main.Scripts.Core.Services;

[Serializable]
public abstract class InteractionAction
{
	protected IInputService inputService;

	public IInteractable Interactable { get; private set; }
	public Interactor Interactor { get; private set; }
	protected PlayerStateModel PlayerStateModel { get; private set; }

	public event Action<InteractionAction> Started;
	public event Action<InteractionAction> Ended;

	public virtual void Init(Interactor interactor, PlayerStateModel playerStateModel, IInputService inputService)
	{
		Interactor = interactor;
		PlayerStateModel = playerStateModel;
		this.inputService = inputService;
	}

	public abstract bool CanInteract(IInteractable interactable);

	public void StartInteract(IInteractable interactable)
	{
		Interactable = interactable;
		// todo: подумать еще раз. точно ли интерактор управляет запуском и прерывание интерактабла? а если интерактбл перестанет быть доступным?
		StartInteractInternal();
		Interactable.StartInteract(Interactor);

		Started?.Invoke(this);
	}

	public void StopInteract()
	{
		StopInteractInternal();
		Interactable.StopInteract(Interactor);

		Ended?.Invoke(this);
	}

	protected virtual void StartInteractInternal() { }
	protected virtual void StopInteractInternal() { }
}
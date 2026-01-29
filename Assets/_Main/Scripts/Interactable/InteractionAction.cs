using System;
using UnityEngine;

[Serializable]
public abstract class InteractionAction
{
	//TODO: оч не нравится, прости меня господь, но пока так.
	public static readonly int State = Animator.StringToHash("State");
	public IInteractable Interactable { get; private set; }
	public Interactor Interactor { get; private set; }
	protected PlayerStateModel StateModel { get; private set; }

	public event Action<InteractionAction> Started;
	public event Action<InteractionAction> Ended;

	public virtual void Init(Interactor interactor, PlayerStateModel playerStateModel)
	{
		Interactor = interactor;
		StateModel = playerStateModel;
	}

	public virtual bool CanInteract(IInteractable interactable)
	{
		return !StateModel.HasState(CharacterState.TRANSITION);
	}

	public void StartInteract(IInteractable interactable, bool immediate = false)
	{
		Interactable = interactable;
		// todo: подумать еще раз. точно ли интерактор управляет запуском и прерывание интерактабла? а если интерактбл перестанет быть доступным?
		StartInteractInternal(immediate);
		Interactable.StartInteract(Interactor);

		Started?.Invoke(this);
	}

	public void StopInteract(bool immediate = false)
	{
		StopInteractInternal(immediate);
		Interactable.StopInteract(Interactor);

		Ended?.Invoke(this);
	}

	protected virtual void StartInteractInternal(bool immediate = false) { }
	protected virtual void StopInteractInternal(bool immediate = false) { }
}
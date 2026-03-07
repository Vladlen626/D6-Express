using System;
using System.Collections.Generic;
using PlatformCore.Core;
using PlatformCore.Services;
using Unity.VisualScripting;
using UnityEngine;

public class Interactor : MonoBehaviour
{
	[SerializeReference]
	[SubclassSelector]
	private List<InteractionAction> actions = new();

	protected readonly List<InteractionAction> activeActions = new();

	protected Interactable selectedInteractable;

	public event Action<InteractionAction> InteractionStarted;
	public event Action<InteractionAction> InteractionEnded;

	public event Action<Interactable> Noticed;
	public event Action<Interactable> Missed;

	public void Initialize(PlayerStateModel playerStateModel, InteractionToStateTable interactionToStateTable)
	{
		foreach (var item in actions)
		{
			item.Init(this, playerStateModel, interactionToStateTable);
		}
	}

	public void TryStopAction<T>() where T : InteractionAction
	{
		for (int i = 0; i < activeActions.Count; i++)
		{
			InteractionAction item = activeActions[i];
			if (typeof(T).IsAssignableFrom(item))
			{
				item.StopInteract();
				break;
			}
		}
	}

	public void StopAllActions(bool immediate = false)
	{
		while (activeActions.Count > 0)
		{
			var index = activeActions.Count - 1;
			var action = activeActions[index];
			activeActions.RemoveAt(index);
			action.StopInteract(immediate);
		}
	}

	public bool CanInteract(Interactable interactable)
	{
		if (!CanInteract())
		{
			return false;
		}

		return interactable != null && !interactable.IsDestroyed() && interactable.CanInteract(this) && TryGetAction(interactable, out var action);
	}

	public void Interact(Interactable interactable, bool immediate = false)
	{
		selectedInteractable = interactable;

		TryGetAction(selectedInteractable, out var action);

		if (action == null)
		{
			return;
		}

		activeActions.Add(action);

		action.Started += OnInteractionStarted;
		action.Ended += OnInteractionEnded;

		action.StartInteract(selectedInteractable, immediate);
	}

	// todo: это говно. надо делать по другому
	protected bool TryGetAction(IInteractable interactable, out InteractionAction action)
	{
		foreach (var item in actions)
		{
			if (item.CanInteract(interactable))
			{
				action = item;
				return true;
			}
		}

		action = null;
		return false;
	}

	protected bool CanInteract()
	{
		return true;
	}

	protected void FireNoticed(Interactable interactable)
	{
		Noticed?.Invoke(interactable);
	}

	protected void FireMissed(Interactable interactable)
	{
		Missed?.Invoke(interactable);
	}

	protected void OnInteractionStarted(InteractionAction interactionAction)
	{
		Locator.Resolve<ILoggerService>().Log(interactionAction + " interaction action started");

		InteractionStarted?.Invoke(interactionAction);
	}

	protected void OnInteractionEnded(InteractionAction interactionAction)
	{
		Locator.Resolve<ILoggerService>().Log(interactionAction + " interaction action ended");

		interactionAction.Ended -= OnInteractionEnded;
		interactionAction.Started -= OnInteractionStarted;

		InteractionEnded?.Invoke(interactionAction);
	}
}
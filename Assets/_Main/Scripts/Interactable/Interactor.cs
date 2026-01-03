using System;
using System.Collections.Generic;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Services;
using Unity.VisualScripting;
using UnityEngine;

public class Interactor : MonoBehaviour
{
	public event Action<InteractionAction> InteractionStarted;
	public event Action<InteractionAction> InteractionEnded;

	public event Action<Interactable> Noticed;
	public event Action<Interactable> Missed;
	
	[SerializeField] private float interactionDistance = 3f;

	[SerializeReference]
	[SubclassSelector]
	private List<InteractionAction> actions = new();

	[SerializeField] private LayerMask interactableLayerMask;

	[SerializeField] private Transform viewTransform;

	// todo: стэк тут ту мач
	private readonly Stack<InteractionAction> actionStack = new();

	private PlayerStateModel playerStateModel;
	private Interactable currentInteractable;
	private IInputService inputService;

	public void Initialize(IInputService inputService, PlayerStateModel playerStateModel)
	{
		this.inputService = inputService;
		this.playerStateModel = playerStateModel;
		this.inputService.OnInteractPressed += OnInteract;
		
		foreach (var item in actions)
		{
			item.Init(this, this.playerStateModel, this.inputService);
		}
	}

	private void OnDisable()
	{
		if (inputService != null)
		{
			inputService.OnInteractPressed -= OnInteract;
		}
	}

	public void StopAllActions()
	{
		while (actionStack.Count > 0)
		{
			var action = actionStack.Pop();

			// todo: кринж, интерактабл должен быть внутри экшена на момент работы
			currentInteractable?.StopInteract(this);
			action.StopInteract(currentInteractable);
		}
	}

	public void StopCurrentAction()
	{
		if (actionStack.Count > 0)
		{
			var action = actionStack.Pop();

			currentInteractable?.StopInteract(this);
			action.StopInteract(currentInteractable);
		}
	}

	private void Update()
	{
		HandleInteraction();
	}

	private void HandleInteraction()
	{
		if (CanInteract())
		{
			Ray ray = new(viewTransform.transform.position, viewTransform.transform.forward);

			if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayerMask))
			{
				Interactable interactable = hit.collider.GetComponent<Interactable>();

				if (interactable != null && interactable.CanInteract(this) && TryGetAction(interactable, out var action))
				{
					currentInteractable = interactable;
					Noticed?.Invoke(currentInteractable);
					return;
				}
			}
		}

		if (currentInteractable != null || currentInteractable.IsDestroyed())
		{
			Missed?.Invoke(currentInteractable);
			currentInteractable = null;
		}
	}

	private void OnInteract()
	{
		if (currentInteractable != null)
		{
			if (!TryGetAction(currentInteractable, out var action))
			{
				return;
			};

			actionStack.Push(action);

			action.Started += OnInteractionStarted;
			action.Ended += OnInteractionEnded;

			currentInteractable.StartInteract(this);
			action.StartInteract(currentInteractable);
		}
	}

	// todo: это говно. надо делать по другому
	private bool TryGetAction(IInteractable interactable, out InteractionAction action)
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

	private bool CanInteract()
	{
		return true;
	}

	private void OnInteractionStarted(InteractionAction interactionAction)
	{
		Locator.Resolve<ILoggerService>().Log(interactionAction + " interaction action started");

		InteractionStarted?.Invoke(interactionAction);
	}

	private void OnInteractionEnded(InteractionAction interactionAction)
	{
		Locator.Resolve<ILoggerService>().Log(interactionAction + " interaction action ended");

		interactionAction.Ended -= OnInteractionEnded;
		interactionAction.Started -= OnInteractionStarted;

		InteractionEnded.Invoke(interactionAction);
	}

	private void OnDrawGizmos()
	{
		var ray = new Ray(viewTransform.position, viewTransform.forward);
		if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayerMask))
		{
			Gizmos.color = Color.green;
		}
		else
		{
			Gizmos.color = Color.red;
		}
		Gizmos.DrawRay(ray.origin, ray.direction * interactionDistance);
	}
}
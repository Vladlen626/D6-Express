using System;
using System.Collections.Generic;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Services;
using UnityEngine;

// todo: переделать на mvc
[RequireComponent(typeof(CharacterStateController))]
public class Interactor : MonoBehaviour
{
	[SerializeField] private float interactionDistance = 3f;

	[SerializeReference]
	[SubclassSelector]
	private List<InteractionAction> actions = new();

	[SerializeField] private LayerMask interactableLayerMask;

	[SerializeField] private Transform viewTransform;

	// todo: стэк тут ту мач
	private readonly Stack<InteractionAction> actionStack = new();

	private Interactable currentInteractable;
	private CharacterStateController characterStateController;

	private IInputService inputService;

	public event Action<InteractionAction> InteractionStarted;
	public event Action<InteractionAction> InteractionEnded;

	public event Action<Interactable> Noticed;
	public event Action<Interactable> Missed;

	private void Awake()
	{
		foreach (var item in actions)
		{
			item.Init(this);
		}

		characterStateController = GetComponent<CharacterStateController>();
	}

	private void Start()
	{
		foreach (var item in actions)
		{
			item.Start();
		}
	}

	private void OnDisable()
	{
		if (inputService != null)
		{
			inputService.OnInteractPressed -= OnInteract;
		}
	}

	public void Initialize(IInputService inInputService)
	{
		inputService = inInputService;

		inputService.OnInteractPressed += OnInteract;
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

		if (currentInteractable != null)
		{
			Missed?.Invoke(currentInteractable);
			currentInteractable = null;
		}
	}

	private void OnInteract()
	{
		if (currentInteractable != null)
		{
			TryGetAction(currentInteractable, out var action);

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

	private void OnInteractionStarted()
	{
		Locator.Resolve<ILoggerService>().Log(actionStack.ToString() + " interaction action started");

		InteractionStarted?.Invoke(actionStack.Peek());
	}

	private void OnInteractionEnded()
	{
		Locator.Resolve<ILoggerService>().Log(actionStack.ToString() + " interaction action ended");

		var action = actionStack.Pop();

		action.Ended -= OnInteractionEnded;
		action.Started -= OnInteractionStarted;

		InteractionEnded.Invoke(action);
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
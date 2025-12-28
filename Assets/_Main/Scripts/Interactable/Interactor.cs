using System;
using System.Collections.Generic;
using _Main.Scripts.Core.Services;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterStateController))]
public class Interactor : MonoBehaviour
{
	[SerializeField] private float interactionDistance = 3f;

	[SerializeReference] [SubclassSelector]
	private List<InteractionAction> actions = new();

	[SerializeField] private LayerMask interactableLayerMask;

	[SerializeField] private Transform viewTransform;

	private GameObject currentInteractable;
	private CharacterStateController characterStateController;

	private IInputService inputService;

	public event Action<GameObject> Noticed;
	public event Action<GameObject> Missed;

	private void Awake()
	{
		foreach (var item in actions)
		{
			item.Init(this);
		}

		characterStateController = GetComponent<CharacterStateController>();
	}

	public void Initialize(IInputService inInputService)
	{
		inputService = inInputService;
	}

	private void Update()
	{
		HandleInteraction();

		if (inputService != null && inputService.IsInteract && currentInteractable)
		{
			OnInteract();
		}
	}

	private void HandleInteraction()
	{
		if (CanInteract())
		{
			Ray ray = new(viewTransform.transform.position, viewTransform.transform.forward);

			if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayerMask))
			{
				IInteractable interactable = hit.collider.GetComponent<IInteractable>();
				// TODO: проверять что есть подходящий экшн
				if (interactable != null && interactable.CanInteract(this))
				{
					currentInteractable = hit.collider.gameObject;
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
		var interactable = currentInteractable.GetComponent<IInteractable>();
		foreach (var item in actions)
		{
			// TODO: подумать еще раз.
			if (item.CanInteract(interactable))
			{
				interactable.StartInteract(this);
				item.StartInteract(interactable);
			}
		}
	}

	private bool CanInteract()
	{
		return characterStateController.State == CharacterState.DEFAULT;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		var ray = new Ray(viewTransform.position, viewTransform.forward);
		Gizmos.DrawRay(ray.origin, ray.direction * interactionDistance);
	}
}
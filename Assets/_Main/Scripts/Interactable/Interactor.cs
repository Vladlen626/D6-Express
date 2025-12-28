using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
	[SerializeField]
	private float interactionDistance = 3f;

	[SerializeReference]
	[SubclassSelector]
	private List<InteractionAction> actions = new();

	[SerializeField]
	private LayerMask interactableLayerMask;

	[SerializeField]
	private Transform viewTransform;

	private GameObject currentInteractable;
	private bool canInteract;

	public event Action<GameObject> Noticed;
	public event Action<GameObject> Missed;

	private void Awake()
	{
		foreach (var item in actions)
		{
			item.Init(this);
		}
	}

	private void Update()
	{
		HandleInteraction();
	}

	private void HandleInteraction()
	{
		Ray ray = new(viewTransform.transform.position, viewTransform.transform.forward);

		Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.green, 3);

		// TODO: не юзать стринг
		if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayerMask))
		{
			IInteractable interactable = hit.collider.GetComponent<IInteractable>();
			// TODO: проверять что есть подходящий экшн
			if (interactable != null && interactable.CanInteract(this))
			{
				currentInteractable = hit.collider.gameObject;
				Noticed(currentInteractable);
				canInteract = true;
				return;
			}
		}

		if (currentInteractable != null)
		{
			var interactable = currentInteractable;
			currentInteractable = null;
			Missed(interactable);
		}
		canInteract = false;
	}

	private void OnJump(InputValue value)
	{
		if (value.isPressed && canInteract && currentInteractable != null)
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
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		var ray = new Ray(viewTransform.position, viewTransform.forward);
		Gizmos.DrawRay(ray.origin, ray.direction * interactionDistance);
	}
}

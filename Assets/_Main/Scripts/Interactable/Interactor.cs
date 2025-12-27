using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
	[SerializeField]
	[Header("Interaction")]
	private float interactionDistance = 3f;

	[SerializeField]
	[Header("Interaction")]
	private LayerMask interactableObjectsLayer;

	[SerializeField]
	private Transform viewTransform;

	private GameObject currentInteractable;
	private bool canInteract;

	public event Action<GameObject> Noticed;
	public event Action<GameObject> Missed;

	private void Update()
	{
		HandleInteraction();
	}

	private void HandleInteraction()
	{
		Ray ray = new(viewTransform.transform.position, viewTransform.transform.forward);

		Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.green, 3);

		if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableObjectsLayer))
		{
			IInteractable interactable = hit.collider.GetComponent<IInteractable>();
			if (interactable != null && interactable.CanInteract())
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
			currentInteractable.GetComponent<IInteractable>().Interact();
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		var ray = new Ray(viewTransform.position, viewTransform.forward);
		Gizmos.DrawRay(ray.origin, ray.direction * interactionDistance);
	}
}

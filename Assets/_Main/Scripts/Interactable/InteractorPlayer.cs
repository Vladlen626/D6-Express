using _Main.Scripts.Core.Services;
using Unity.VisualScripting;
using UnityEngine;

public class InteractorPlayer : Interactor
{
	[SerializeField]
	private float interactionDistance = 3f;

	[SerializeField]
	private LayerMask interactableLayerMask;

	[SerializeField]
	private Transform viewTransform;

	private IInputService inputService;

	public void Initialize(IInputService inputService, PlayerStateModel playerStateModel)
	{
		Initialize(playerStateModel);

		this.inputService = inputService;
		this.inputService.OnInteractPressed += OnInteract;
	}

	private void OnDisable()
	{
		if (inputService != null)
		{
			inputService.OnInteractPressed -= OnInteract;
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
				if (hit.collider.TryGetComponent<Interactable>(out var interactable) && CanInteract(interactable))
				{
					selectedInteractable = interactable;
					FireNoticed(selectedInteractable);
					return;
				}
			}
		}

		if (selectedInteractable != null || selectedInteractable.IsDestroyed())
		{
			FireMissed(selectedInteractable);
			selectedInteractable = null;
		}
	}

	private void OnInteract()
	{
		if (selectedInteractable != null)
		{
			Interact(selectedInteractable);
		}
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
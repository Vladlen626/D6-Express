using _Main.Scripts.Core.Services;
using PlatformCore.Services.Audio;
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

	public bool InteractableDetectionEnabled { get; private set; } = true;

	private IInputService inputService;
	private IAudioService audioService;

	public void Initialize(
		IInputService inputService,
		IAudioService audioService,
		PlayerStateModel playerStateModel,
		InteractionToStateTable interactionToStateTable)
	{
		Initialize(playerStateModel, interactionToStateTable);

		this.inputService = inputService;
		this.audioService = audioService;
		this.inputService.OnInteractPressed += OnInteract;
	}

	public void EnableInteractableDetection()
	{
		InteractableDetectionEnabled = true;
	}

	public void DisableInteractableDetection()
	{
		InteractableDetectionEnabled = false;
	}

	private void OnEnable()
	{
		if (inputService != null)
		{
			inputService.OnInteractPressed += OnInteract;
		}
	}

	private void OnDisable()
	{
		if (selectedInteractable != null)
		{
			FireMissed(selectedInteractable);
		}
		selectedInteractable = null;

		if (inputService != null)
		{
			inputService.OnInteractPressed -= OnInteract;
		}
	}

	private void Update()
	{
		TryFindInteractable();
	}

	private void TryFindInteractable()
	{
		if (InteractableDetectionEnabled)
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
			audioService?.PlaySound(SoundNames.Button);
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

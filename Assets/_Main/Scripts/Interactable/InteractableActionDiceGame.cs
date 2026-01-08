using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class InteractableActionDiceGame : InteractionAction
{
	private Vector3 lastPos;
	private Quaternion lastRot;

	public override bool CanInteract(IInteractable interactable)
	{
		return interactable is InteractableDiceGame && PlayerStateModel.HasState(CharacterState.DEFAULT) && base.CanInteract(interactable);
	}

	protected override async void StartInteractInternal()
	{
		PlayerStateModel.TryAddState(CharacterState.TRANSITION);

		lastPos = Interactor.InteractionRoot.position;
		lastRot = Interactor.InteractionRoot.rotation;

		var interactableDiceGame = Interactable as InteractableDiceGame;
		
		Interactor.InteractionRoot.SetPositionAndRotation(interactableDiceGame.SitTfm.position, interactableDiceGame.SitTfm.rotation);

		PlayerStateModel.TryRemoveState(CharacterState.TRANSITION);
		PlayerStateModel.TryAddState(CharacterState.DICE_GAME);

		inputService.OnInteractPerformed += OnInteractHoldCompleted;
	}

	protected override async void StopInteractInternal()
	{
		inputService.OnInteractPerformed -= OnInteractHoldCompleted;
		
		Interactor.InteractionRoot.SetPositionAndRotation(lastPos, lastRot);

		PlayerStateModel.TryRemoveState(CharacterState.DICE_GAME);
	}

	private void OnInteractHoldCompleted()
	{
		if (!PlayerStateModel.HasState(CharacterState.DICE_GAME))
		{
			return;
		}

		StopInteract();
	}
}
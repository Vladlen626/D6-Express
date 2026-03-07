using System;
using UnityEngine;

[Serializable]
public class InteractableActionDiceGame : InteractionAction
{
	private Vector3 lastPos;
	private Quaternion lastRot;

	public override bool CanInteract(IInteractable interactable)
	{
		return interactable.Type == InteractionType.PLAY_DICE && base.CanInteract(interactable);
	}

	protected override async void StartInteractInternal(bool immediate = false)
	{
		(Interactor as InteractorPlayer).DisableInteractableDetection();

		StateModel.TryAddState(CharacterState.TRANSITION);

		lastPos = Interactor.transform.position;
		lastRot = Interactor.transform.rotation;

		var interactableDiceGame = Interactable as InteractableDiceGame;

		Interactor.transform.SetPositionAndRotation(interactableDiceGame.SitTfm.position, interactableDiceGame.SitTfm.rotation);

		StateModel.TryRemoveState(CharacterState.TRANSITION);
		StateModel.TryAddState(CharacterState.DICE_GAME);
	}

	protected override async void StopInteractInternal(bool immediate = false)
	{
		(Interactor as InteractorPlayer).EnableInteractableDetection();

		Interactor.transform.SetPositionAndRotation(lastPos, lastRot);

		StateModel.TryRemoveState(CharacterState.DICE_GAME);
	}
}
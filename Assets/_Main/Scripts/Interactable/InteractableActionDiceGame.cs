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
		return interactable is InteractableDiceGame && PlayerStateModel.HasState(CharacterState.DEFAULT);
	}

	protected override async void StartInteractInternal(IInteractable interactable)
	{
		PlayerStateModel.TryAddState(CharacterState.TRANSITION);

		lastPos = Interactor.transform.position;
		lastRot = Interactor.transform.rotation;

		var interactableDiceGame = interactable as InteractableDiceGame;

		var moveTask = Interactor.transform.DOMove(interactableDiceGame.SitTfm.position, 1).ToUniTask();
		var rotateTask = Interactor.transform.DORotateQuaternion(interactableDiceGame.SitTfm.rotation, 1).ToUniTask();

		await UniTask.WhenAll(moveTask, rotateTask);

		PlayerStateModel.TryRemoveState(CharacterState.TRANSITION);
		PlayerStateModel.TryAddState(CharacterState.DICE_GAME);

		inputService.OnMoved += OnMoved;
	}

	protected override async void StopInteractInternal(IInteractable interactable)
	{
		inputService.OnMoved -= OnMoved;

		var moveTask = Interactor.transform.DOMove(lastPos, 0.25f).ToUniTask();
		var rotateTask = Interactor.transform.DORotateQuaternion(lastRot, 0.25f).ToUniTask();

		await UniTask.WhenAll(moveTask, rotateTask);

		PlayerStateModel.TryRemoveState(CharacterState.DICE_GAME);
	}

	private void OnMoved(Vector2 dir)
	{
		StopInteract(null);
	}
}
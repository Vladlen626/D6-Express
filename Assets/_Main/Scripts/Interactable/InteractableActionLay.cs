using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class InteractableActionLay : InteractionAction
{
	private Vector3 lastPos;

	public override bool CanInteract(IInteractable interactable)
	{
		return interactable is InteractableLayable && !PlayerStateModel.HasState(CharacterState.LAYING);
	}

	protected override async void StartInteractInternal()
	{
		PlayerStateModel.TryAddState(CharacterState.TRANSITION);

		lastPos = Interactor.InteractionRoot.position;

		var layable = Interactable as InteractableLayable;

		var moveTask = Interactor.InteractionRoot.DOMove(layable.SitTfm.position, 1).ToUniTask();
		var rotateTask = Interactor.InteractionRoot.DORotateQuaternion(layable.SitTfm.rotation, 1).ToUniTask();

		await UniTask.WhenAll(moveTask, rotateTask);

		PlayerStateModel.TryRemoveState(CharacterState.TRANSITION);
		PlayerStateModel.TryAddState(CharacterState.LAYING);
		inputService.OnMoved += OnMoved;
	}

	protected async override void StopInteractInternal()
	{
		inputService.OnMoved -= OnMoved;

		var moveTask = Interactor.InteractionRoot.DOMove(lastPos, 0.25f).ToUniTask();
		var rotateTask = Interactor.InteractionRoot.DORotateQuaternion(Quaternion.identity, 0.25f).ToUniTask();

		await UniTask.WhenAll(moveTask, rotateTask);

		PlayerStateModel.TryRemoveState(CharacterState.LAYING);
	}

	private async void OnMoved(Vector2 dir)
	{
		var moveTask = Interactor.InteractionRoot.DOMove(lastPos, 0.25f).ToUniTask();
		var rotateTask = Interactor.InteractionRoot.DORotateQuaternion(Quaternion.identity, 0.25f).ToUniTask();

		await UniTask.WhenAll(moveTask, rotateTask);

		StopInteract();
	}
}
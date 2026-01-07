using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class InteractableActionSit : InteractionAction
{
	private Vector3 lastPos;

	public override bool CanInteract(IInteractable interactable)
	{
		return interactable is InteractableSittable && !PlayerStateModel.HasState(CharacterState.SITTING);
	}

	protected override async void StartInteractInternal()
	{
		PlayerStateModel.TryAddState(CharacterState.TRANSITION);

		lastPos = Interactor.InteractionRoot.position;

		var sittable = Interactable as InteractableSittable;

		var moveTask = Interactor.InteractionRoot.DOMove(sittable.SitTfm.position, 1).ToUniTask();
		var rotateTask = Interactor.InteractionRoot.DORotateQuaternion(sittable.SitTfm.rotation, 1).ToUniTask();

		await UniTask.WhenAll(moveTask, rotateTask);

		PlayerStateModel.TryRemoveState(CharacterState.TRANSITION);
		PlayerStateModel.TryAddState(CharacterState.SITTING);

		inputService.OnMoved += OnMoved;
	}

	protected override async void StopInteractInternal()
	{
		inputService.OnMoved -= OnMoved;

		await Interactor.InteractionRoot.DOMove(lastPos, 0.25f).ToUniTask();

		PlayerStateModel.TryRemoveState(CharacterState.SITTING);
	}

	private async void OnMoved(Vector2 dir)
	{
		await Interactor.InteractionRoot.DOMove(lastPos, 0.25f).ToUniTask();

		StopInteract();
	}
}
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

	protected override async void StartInteractInternal(IInteractable interactable)
	{
		PlayerStateModel.TryAddState(CharacterState.TRANSITION);

		lastPos = Interactor.transform.position;

		var sittable = interactable as InteractableSittable;

		var moveTask = Interactor.transform.DOMove(sittable.SitTfm.position, 1).ToUniTask();
		var rotateTask = Interactor.transform.DORotateQuaternion(sittable.SitTfm.rotation, 1).ToUniTask();

		await UniTask.WhenAll(moveTask, rotateTask);

		PlayerStateModel.TryRemoveState(CharacterState.TRANSITION);
		PlayerStateModel.TryAddState(CharacterState.SITTING);

		inputService.OnMoved += OnMoved;
	}

	protected override async void StopInteractInternal(IInteractable interactable)
	{
		inputService.OnMoved -= OnMoved;

		await Interactor.transform.DOMove(lastPos, 0.25f).ToUniTask();

		PlayerStateModel.TryRemoveState(CharacterState.SITTING);
	}

        stateController.TryRemoveState(CharacterState.SITTING);
    }

    private async void OnMoved(Vector2 dir)
    {
        await Interactor.transform.DOMove(lastPos, 0.25f).ToUniTask();

        StopInteract(null);
    }
}
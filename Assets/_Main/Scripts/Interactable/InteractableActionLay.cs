using System;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlatformCore.Core;
using UnityEngine;

[Serializable]
public class InteractableActionLay : InteractionAction
{
	protected Vector3 lastPos;

	public override bool CanInteract(IInteractable interactable)
	{
		return interactable.Type == InteractionType.LAY && !StateModel.HasState(CharacterState.LAYING) && base.CanInteract(interactable);
	}

	protected override async void StartInteractInternal()
	{
		StateModel.TryAddState(CharacterState.TRANSITION);

		lastPos = Interactor.transform.position;

		var layable = Interactable as InteractableLayable;

		// var moveTask = Interactor.transform.DOMove(layable.SitTfm.position, 1).ToUniTask();
		// var rotateTask = Interactor.transform.DORotateQuaternion(layable.SitTfm.rotation, 1).ToUniTask();

		// await UniTask.WhenAll(moveTask, rotateTask);
		Interactor.transform.SetPositionAndRotation(layable.SitTfm.position, layable.SitTfm.rotation);

		StateModel.TryRemoveState(CharacterState.TRANSITION);
		StateModel.TryAddState(CharacterState.LAYING);
	}

	protected async override void StopInteractInternal()
	{
		// var moveTask = Interactor.InteractionRoot.DOMove(lastPos, 0.25f).ToUniTask();
		// var rotateTask = Interactor.InteractionRoot.DORotateQuaternion(Quaternion.identity, 0.25f).ToUniTask();

		// await UniTask.WhenAll(moveTask, rotateTask);
		Interactor.transform.SetPositionAndRotation(lastPos, Quaternion.identity);

		StateModel.TryRemoveState(CharacterState.LAYING);
	}
}
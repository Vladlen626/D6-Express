using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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

		var moveTask = Interactor.transform.DOMove(layable.SitTfm.position, .25f).ToUniTask();
		var rotateTask = Interactor.transform.DORotateQuaternion(layable.SitTfm.rotation, .25f).ToUniTask();

		await UniTask.WhenAll(moveTask, rotateTask);

		StateModel.TryRemoveState(CharacterState.TRANSITION);
		StateModel.TryAddState(CharacterState.LAYING);
	}

	protected async override void StopInteractInternal()
	{
		StateModel.TryAddState(CharacterState.TRANSITION);

		var moveTask = Interactor.transform.DOMove(lastPos, 0.25f).ToUniTask();
		var rotateTask = Interactor.transform.DORotateQuaternion(Quaternion.identity, 0.25f).ToUniTask();

		// todo: так делать нельзя
		var rotateHeadTask = Interactor.GetComponent<CharacterView>().Head.transform.DOLocalRotateQuaternion(Quaternion.identity, 0.25f).ToUniTask();

		await UniTask.WhenAll(moveTask, rotateTask, rotateHeadTask);

		StateModel.TryRemoveState(CharacterState.TRANSITION);
		StateModel.TryRemoveState(CharacterState.LAYING);
	}
}
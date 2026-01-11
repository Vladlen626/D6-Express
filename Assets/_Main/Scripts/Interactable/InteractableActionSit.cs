using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class InteractableActionSit : InteractionAction
{
	protected Vector3 lastPos;

	public override bool CanInteract(IInteractable interactable)
	{
		return interactable.Type == InteractionType.SIT && !StateModel.HasState(CharacterState.SITTING) && base.CanInteract(interactable);
	}

	protected override async void StartInteractInternal()
	{
		StateModel.TryAddState(CharacterState.TRANSITION);

		lastPos = Interactor.transform.position;

		var sittable = Interactable as InteractableSittable;

		// var moveTask = Interactor.transform.DOMove(sittable.SitTfm.position - Interactor.OffsetFromOrigin, 10).ToUniTask();
		// var rotateTask = Interactor.transform.DORotateQuaternion(sittable.SitTfm.rotation, 10).ToUniTask();
		Interactor.transform.SetPositionAndRotation(sittable.SitTfm.position, sittable.SitTfm.rotation);
		
		Interactor.GetComponent<Animator>().SetInteger("State", 1);
		// await UniTask.WhenAll(moveTask, rotateTask);

		StateModel.TryRemoveState(CharacterState.TRANSITION);
		StateModel.TryAddState(CharacterState.SITTING);
	}

	protected override async void StopInteractInternal()
	{
		Interactor.GetComponent<Animator>().SetInteger("State", 0);

		// await Interactor.InteractionRoot.DOMove(lastPos, 0.25f).ToUniTask();
		Interactor.transform.position = lastPos;

		StateModel.TryRemoveState(CharacterState.SITTING);
	}
}
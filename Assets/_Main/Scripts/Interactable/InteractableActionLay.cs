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

	protected override async void StartInteractInternal(bool immediate = false)
	{
		StateModel.TryAddState(CharacterState.TRANSITION);

		lastPos = Interactor.transform.position;

		var layable = Interactable as InteractableLayable;

		if (immediate)
		{
			Interactor.transform.SetPositionAndRotation(layable.SitTfm.position, layable.SitTfm.rotation);
		}
		else
		{
			var moveTask = Interactor.transform.DOMove(layable.SitTfm.position, .25f).AsyncWaitForCompletion().AsUniTask(); 
			var rotateTask = Interactor.transform.DORotateQuaternion(layable.SitTfm.rotation, .25f).AsyncWaitForCompletion().AsUniTask();
			Interactor.GetComponent<CharacterView>().Animator.SetInteger(State, 2);
			await UniTask.WhenAll(moveTask, rotateTask);
		}

		StateModel.TryRemoveState(CharacterState.TRANSITION);
		StateModel.TryAddState(CharacterState.LAYING);
	}

	protected async override void StopInteractInternal(bool immediate = false)
	{
		StateModel.TryAddState(CharacterState.TRANSITION);

		if (immediate)
		{
			Interactor.GetComponent<CharacterView>().Head.transform.localRotation = Quaternion.identity;
			Interactor.transform.SetPositionAndRotation(lastPos, Quaternion.identity);
		}
		else
		{
			var moveTask = Interactor.transform.DOMove(lastPos, 0.25f).AsyncWaitForCompletion().AsUniTask();
			var rotateTask = Interactor.transform.DORotateQuaternion(Quaternion.identity, 0.25f).AsyncWaitForCompletion().AsUniTask();

			// todo: так делать нельзя
			var rotateHeadTask = Interactor.GetComponent<CharacterView>().Head.transform.DOLocalRotateQuaternion(Quaternion.identity, 0.25f).AsyncWaitForCompletion().AsUniTask();
			await UniTask.WhenAll(moveTask, rotateTask, rotateHeadTask);
		}

		Interactor.GetComponent<CharacterView>().Animator.SetInteger(State, 0);
		StateModel.TryRemoveState(CharacterState.TRANSITION);
		StateModel.TryRemoveState(CharacterState.LAYING);
	}
}
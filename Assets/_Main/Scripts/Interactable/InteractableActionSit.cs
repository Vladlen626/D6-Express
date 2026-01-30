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
		return interactable.Type == InteractionType.SIT && base.CanInteract(interactable);
	}

	protected override async void StartInteractInternal(bool immediate = false)
	{
		StateModel.TryAddState(CharacterState.TRANSITION);

		lastPos = Interactor.transform.position;

		var sittable = Interactable as InteractableSittable;

		if (immediate)
		{
			Interactor.transform.SetPositionAndRotation(sittable.SitTfm.position, sittable.SitTfm.rotation);
		}
		else
		{
			var moveTask = Interactor.transform.DOMove(sittable.SitTfm.position, 0.25f).AsyncWaitForCompletion().AsUniTask();
			var rotateTask = Interactor.transform.DORotateQuaternion(sittable.SitTfm.rotation, 0.25f).AsyncWaitForCompletion().AsUniTask();

			
			// TODO: НУжна какая-то более адекватная тема, и инты поменять на енам
			Interactor.GetComponent<CharacterView>().Animator.SetInteger(State, 1);
			await UniTask.WhenAll(moveTask, rotateTask);
		}

		StateModel.TryRemoveState(CharacterState.TRANSITION);
		StateModel.TryAddState(CharacterState.SITTING);
	}

	protected override async void StopInteractInternal(bool immediate = false)
	{
		StateModel.TryAddState(CharacterState.TRANSITION);

		// TODO: НУжна какая-то более адекватная тема, и инты поменять на енам
		Interactor.GetComponent<CharacterView>().Animator.SetInteger(State, 0);

		if (immediate)
		{
			Interactor.GetComponent<CharacterView>().Head.transform.localRotation = Quaternion.identity;
			Interactor.transform.position = lastPos;
		}
		else
		{
			var moveTask = Interactor.transform.DOMove(lastPos, 0.25f).AsyncWaitForCompletion().AsUniTask();

			// todo: так делать нельзя
			var rotateTask = Interactor.GetComponent<CharacterView>().Head.transform
				.DOLocalRotateQuaternion(Quaternion.identity, 0.25f)
				.AsyncWaitForCompletion().AsUniTask();

			await UniTask.WhenAll(moveTask, rotateTask);
		}

		StateModel.TryRemoveState(CharacterState.TRANSITION);
		StateModel.TryRemoveState(CharacterState.SITTING);
	}
}
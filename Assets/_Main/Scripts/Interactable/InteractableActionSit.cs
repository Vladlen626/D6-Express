using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class InteractableActionSit : InteractionAction
{
	private static readonly int State = Animator.StringToHash("State");
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

		var moveTask = Interactor.transform.DOMove(sittable.SitTfm.position, 0.25f).AsyncWaitForCompletion().AsUniTask();
		var rotateTask = Interactor.transform.DORotateQuaternion(sittable.SitTfm.rotation, 0.25f).AsyncWaitForCompletion().AsUniTask();

		Interactor.GetComponent<Animator>().SetInteger(State, 1);
		await UniTask.WhenAll(moveTask, rotateTask);

		StateModel.TryRemoveState(CharacterState.TRANSITION);
		StateModel.TryAddState(CharacterState.SITTING);
	}

	protected override async void StopInteractInternal()
	{
		StateModel.TryAddState(CharacterState.TRANSITION);

		Interactor.GetComponent<Animator>().SetInteger(State, 0);

		var moveTask = Interactor.transform.DOMove(lastPos, 0.25f).AsyncWaitForCompletion().AsUniTask();

		// todo: так делать нельзя
		var rotateTask = Interactor.GetComponent<CharacterView>().Head.transform
			.DOLocalRotateQuaternion(Quaternion.identity, 0.25f)
			.AsyncWaitForCompletion().AsUniTask();

		await UniTask.WhenAll(moveTask, rotateTask);

		StateModel.TryRemoveState(CharacterState.TRANSITION);
		StateModel.TryRemoveState(CharacterState.SITTING);
	}
}
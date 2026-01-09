using System;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlatformCore.Core;
using UnityEngine;

[Serializable]
public class InteractableActionDiceGame : InteractionAction
{
	private Vector3 lastPos;
	private Quaternion lastRot;

	public override bool CanInteract(IInteractable interactable)
	{
		return interactable.Type == InteractionType.PLAY_DICE && StateModel.HasState(CharacterState.DEFAULT) && base.CanInteract(interactable);
	}

	protected override async void StartInteractInternal()
	{
		StateModel.TryAddState(CharacterState.TRANSITION);

		lastPos = Interactor.transform.position;
		lastRot = Interactor.transform.rotation;

		var interactableDiceGame = Interactable as InteractableDiceGame;

		// var moveTask = Interactor.transform.DOMove(interactableDiceGame.SitTfm.position, 1).ToUniTask();
		// var rotateTask = Interactor.transform.DORotateQuaternion(interactableDiceGame.SitTfm.rotation, 1).ToUniTask();

		// await UniTask.WhenAll(moveTask, rotateTask);
		Interactor.transform.SetPositionAndRotation(interactableDiceGame.SitTfm.position, interactableDiceGame.SitTfm.rotation);

		StateModel.TryRemoveState(CharacterState.TRANSITION);
		StateModel.TryAddState(CharacterState.DICE_GAME);

		Locator.Resolve<IInputService>().OnInteractPerformed += OnInteractHoldCompleted;
	}

	protected override async void StopInteractInternal()
	{
		Locator.Resolve<IInputService>().OnInteractPerformed -= OnInteractHoldCompleted;

		// var moveTask = Interactor.transform.DOMove(lastPos, 0.25f).ToUniTask();
		// var rotateTask = Interactor.transform.DORotateQuaternion(lastRot, 0.25f).ToUniTask();

		// await UniTask.WhenAll(moveTask, rotateTask);
		Interactor.transform.SetPositionAndRotation(lastPos, lastRot);

		StateModel.TryRemoveState(CharacterState.DICE_GAME);
	}

	private void OnInteractHoldCompleted()
	{
		if (!StateModel.HasState(CharacterState.DICE_GAME))
		{
			return;
		}

		StopInteract();
	}
}
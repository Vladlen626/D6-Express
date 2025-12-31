using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class InteractableActionDiceGame : InteractionAction
{
	private CharacterStateController stateController;
	private Vector3 lastPos;
	private Quaternion lastRot;

	public override void Init(Interactor interactor)
	{
		base.Init(interactor);

		stateController = interactor.GetComponent<CharacterStateController>();
	}

	public override bool CanInteract(IInteractable interactable)
	{
		return interactable is InteractableDiceGame && stateController.State == CharacterState.DEFAULT;
	}

	protected override async void StartInteractInternal(IInteractable interactable)
	{
		stateController.TryEnterState(CharacterState.TRANSITION);

		lastPos = Interactor.transform.position;
		lastRot = Interactor.transform.rotation;

		var interactableDiceGame = interactable as InteractableDiceGame;

		var moveTask = Interactor.transform.DOMove(interactableDiceGame.SitTfm.position, 1).ToUniTask();
		var rotateTask = Interactor.transform.DORotateQuaternion(interactableDiceGame.SitTfm.rotation, 1).ToUniTask();

		await UniTask.WhenAll(moveTask, rotateTask);

		stateController.TryEnterState(CharacterState.DICE_GAME);

		inputService.OnMoved += OnMoved;
	}

	protected override async void StopInteractInternal(IInteractable interactable)
	{
		inputService.OnMoved -= OnMoved;

		var moveTask = Interactor.transform.DOMove(lastPos, 0.25f).ToUniTask();
		var rotateTask = Interactor.transform.DORotateQuaternion(lastRot, 0.25f).ToUniTask();
	
		await UniTask.WhenAll(moveTask, rotateTask);

		stateController.TryEnterState(CharacterState.DEFAULT);
	}

	private void OnMoved(Vector2 dir)
	{
		StopInteract(null);
	}
}
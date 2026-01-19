using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;

[Serializable]
public class InteractableActionSpeak : InteractionAction
{
	public int Id { get; private set; } = -1;

	public override bool CanInteract(IInteractable interactable)
	{
		return interactable.Type == InteractionType.SPEAK && StateModel.HasState(CharacterState.DEFAULT);
	}

	protected override async void StartInteractInternal()
	{
		base.StartInteractInternal();

		// todo: выглядит сомнительно
		var speakable = Interactable as InteractableSpeakable;
		Id = speakable.Id;

		var targetable = speakable.GetComponent<Targetable>();

		var rotateTarget = targetable == null ? speakable.transform : targetable.CameraTarget;

		StateModel.TryAddState(CharacterState.SPEAKING);

		var playerView = Interactor.GetComponent<PlayerView>();
		await playerView.Head.DOLookAt(rotateTarget.position, .25f).ToUniTask();
	}

	protected override async void StopInteractInternal()
	{
		Id = -1;

		StateModel.TryRemoveState(CharacterState.SPEAKING);

		base.StopInteractInternal();
	}
}
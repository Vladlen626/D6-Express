using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Services.UI;

public class SpeechController : BaseContextController<UISpeechView>
{
	private readonly Interactor interactor;
	private readonly SpeechModel speechModel;

	private Speech currentSpeech;
	private InteractionAction speechAction;

	public SpeechController(IUIService uiService, Interactor interactor, SpeechModel speechModel) : base(uiService)
	{
		this.interactor = interactor;
		this.speechModel = speechModel;
	}

	protected override void OnActivate()
	{
		base.OnActivate();

		interactor.InteractionStarted += OnInteractionStarted;
	}

	protected override void OnDeactivate()
	{
		interactor.InteractionStarted -= OnInteractionStarted;

		base.OnDeactivate();
	}

	private void OnInteractionStarted(InteractionAction action)
	{
		// TODO: запуск спича не должен быть тут
		if (action is InteractableActionSpeak speechAction)
		{
			this.speechAction = speechAction;

			_context.Show();

			currentSpeech = speechModel.GetSpeech(speechAction.Id);

			currentSpeech.Blackboard[SpeechBlackboardBaseKeys.USER] = action.Interactor.gameObject;
			// todo: ты охуел?
			currentSpeech.Blackboard[SpeechBlackboardBaseKeys.TARGET] = (action.Interactable as InteractableSpeakable).gameObject;

			currentSpeech.Finished += OnSpeechFinished;
			currentSpeech.RequestStart();
		}
	}

	private void OnSpeechFinished()
	{
		currentSpeech.Finished -= OnSpeechFinished;

		// todo плохая идея завершать интеракцию изнутри контроллера
		speechAction.StopInteract();

		_context.Hide();

		speechAction = null;
		currentSpeech = null;
	}
}
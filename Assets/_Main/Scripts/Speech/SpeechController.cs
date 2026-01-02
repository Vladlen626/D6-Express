using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Services.UI;

public class SpeechController : BaseContextController<UISpeechView>
{
	private readonly IInputService inputService;
	private readonly Interactor interactor;
	private readonly SpeechModel speechModel;

	private Speech currentSpeech;
	private InteractionAction speechAction;

	public SpeechController(IUIService uiService, IInputService inputService, Interactor interactor, SpeechModel speechModel) : base(uiService)
	{
		this.inputService = inputService;
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
		if (action is InteractableActionSpeak speechAction)
		{
			this.speechAction = speechAction;
			
			// todo: возможно должно решаться в стейте персонажа
			inputService.DisableCameraInputs();
			inputService.DisablePlayerInputs();

			_context.Show();

			currentSpeech = speechModel.GetSpeech(speechAction.Id);
			currentSpeech.Finished += OnSpeechFinished;
			currentSpeech.RequestStart();
		}
	}

	private void OnSpeechFinished()
	{
		currentSpeech.Finished -= OnSpeechFinished;

		// todo никакого нулл, вообще не стоит в стопе передавать интерактбла
		// todo плохая идея завершать интеракцию изнутри контроллера
		speechAction.StopInteract(null);

		inputService.EnableCameraInputs();
		inputService.EnablePlayerInputs();

		_context.Hide();

		speechAction = null;
		currentSpeech = null;
	}
}
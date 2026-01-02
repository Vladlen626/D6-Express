using _Main.Scripts.Core.Services;
using PlatformCore.Services.UI;

public static class SpeechFactory
{
	public static SpeechController GetSpeechController(IUIService uiService, IInputService inputService, PlayerView playerView, LevelModel levelModel)
	{
		// todo бл
		var speechBuyTicket = new Speech(0);
		var speechNodeConductorSaysHi = new SpeechNodeShowText("Здравствуйте, пассажир! Добро пожаловать на борт D6-Express.").Init(speechBuyTicket);
		var speechNodeMoveToTrain = new SpeechNodeDo(() => levelModel.SetLevelState(LevelState.TRAIN)).After(speechNodeConductorSaysHi).Init(speechBuyTicket);
		speechBuyTicket.SetRootNode(speechNodeConductorSaysHi);

		var speeches = new Speech[]
		{
			speechBuyTicket
		};

		var speechModel = new SpeechModel(speeches);

		var interactor = playerView.GetComponent<Interactor>();

		return new SpeechController(uiService, inputService, interactor, speechModel);
	}
}

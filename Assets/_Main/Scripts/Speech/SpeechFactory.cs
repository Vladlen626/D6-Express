using _Main.Scripts.Core.Services;
using PlatformCore.Services.UI;

public static class SpeechFactory
{
	public static SpeechController GetSpeechController(IUIService uiService, PlayerView playerView, LevelModel levelModel)
	{
		// todo бл
		var speechBuyTicket = new Speech(0);
		var speechNodeConductorSaysHi = new SpeechNodeShowTextLine("Здравствуйте, пассажир! Добро пожаловать на борт D6-Express.").Init(speechBuyTicket);
		var speechNodeMoveToTrain = new SpeechNodeDo(() => levelModel.SetLevelState(LevelState.TRAIN)).After(speechNodeConductorSaysHi).Init(speechBuyTicket);
		speechBuyTicket.SetRootNode(speechNodeConductorSaysHi);

		var speechPassengerGeneric = new Speech(1);
		var speechNodeRanomText = new SpeechNodeShowTextLineRandom(
			"Какой сегодня прекрасный день для путешествия!",
			"Надеюсь, поездка будет комфортной и приятной.",
			"Интересно, какие приключения ждут меня впереди?",
			"Я всегда мечтал посетить новые места на поезде.",
			"Люблю звук колес по рельсам, он такой успокаивающий.",
			"Кто-то украл твой сладкий рулетик?"
		).Init(speechPassengerGeneric);
		speechPassengerGeneric.SetRootNode(speechNodeRanomText);

		var speeches = new Speech[]
		{
			speechBuyTicket,
			speechPassengerGeneric
		};

		var speechModel = new SpeechModel(speeches);

		var interactor = playerView.GetComponent<Interactor>();

		return new SpeechController(uiService, interactor, speechModel);
	}
}

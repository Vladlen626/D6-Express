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
		var speechNodeRandomText = new SpeechNodeShowTextLineRandom(
			"Какой сегодня прекрасный день для путешествия!",
			"Надеюсь, поездка будет комфортной и приятной.",
			"Интересно, какие приключения ждут меня впереди?",
			"Я всегда мечтал посетить новые места на поезде.",
			"Люблю звук колес по рельсам, он такой успокаивающий.",
			"Кто-то украл твой сладкий рулетик?"
		).Init(speechPassengerGeneric);
		speechPassengerGeneric.SetRootNode(speechNodeRandomText);

		var speechPassengerComrade = new Speech(2);
		var speechNodeComradeText = new SpeechNodeShowTextLine("Я вас категорически приветствую!").Init(speechPassengerGeneric);
		speechPassengerComrade.SetRootNode(speechNodeComradeText);

		var speechPassengerAnecdote = new Speech(3);
		var speechNodeAnecdoteText1 = new SpeechNodeShowTextLine("Заходит пациент к доктору и говорит").Init(speechPassengerAnecdote);
		var speechNodeAnecdoteText2 = new SpeechNodeShowTextLine("Доктор, у меня член чешется").After(speechNodeAnecdoteText1).Init(speechPassengerAnecdote);
		var speechNodeAnecdoteText3 = new SpeechNodeShowTextLine("Доктор отвечает \"Мой чаще\"").After(speechNodeAnecdoteText2).Init(speechPassengerAnecdote);
		var speechNodeAnecdoteText4 = new SpeechNodeShowTextLine("А пациент такой \"Нет, мой\"").After(speechNodeAnecdoteText3).Init(speechPassengerAnecdote);
		var speechNodeAnecdoteText5 = new SpeechNodeShowTextLine("Ха-ха-ха-ха!").After(speechNodeAnecdoteText4).Init(speechPassengerAnecdote);
		speechPassengerAnecdote.SetRootNode(speechNodeAnecdoteText1);

		var speeches = new Speech[]
		{
			speechBuyTicket,
			speechPassengerGeneric,
			speechPassengerComrade,
			speechPassengerAnecdote
		};

		var speechModel = new SpeechModel(speeches);

		var interactor = playerView.GetComponent<Interactor>();

		return new SpeechController(uiService, interactor, speechModel);
	}
}

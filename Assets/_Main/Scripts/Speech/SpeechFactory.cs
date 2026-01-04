using _Main.Scripts.Core.Services;
using PlatformCore.Services.UI;

public static class SpeechFactory
{
    public static SpeechController GetSpeechController(IUIService uiService, PlayerModel playerModel, PlayerView playerView, LevelModel levelModel)
    {
        // todo бл
        var speechBuyTicket = GetConductorSpeech(playerModel, levelModel);
        var speechPassengerGeneric = GetGenericPassengerSpeech();
        var speechPassengerComrade = GetComradeSpeech(speechPassengerGeneric);
        var speechPassengerAnecdote = GetAnecdoteSpeech();
        var speechShopkeeper = GetShopkeeperSpeech(playerModel);

        var speeches = new Speech[]
        {
            speechBuyTicket,
            speechPassengerGeneric,
            speechPassengerComrade,
            speechPassengerAnecdote,
            speechShopkeeper
        };

        var speechModel = new SpeechModel(speeches);

        var interactor = playerView.GetComponent<Interactor>();

        return new SpeechController(uiService, interactor, speechModel);
    }


    private static Speech GetConductorSpeech(PlayerModel playerModel, LevelModel levelModel)
    {
        var speechBuyTicket = new Speech(0);
        var speechNodeConductorSaysHi = new SpeechNodeShowTextLine("Так-так").Init(speechBuyTicket);
        var speechNodeHasMoney = new SpeechNodeShowTextLine("Добро пожаловать на борт D6-Express.").Init(speechBuyTicket);
        var speechNodeHasNoMoney = new SpeechNodeShowTextLine("Проваливай, нищий обрыган!").Init(speechBuyTicket);
        var speechNodeConditional = new SpeechNodeConditional(() => playerModel.InventoryModel.CashCount >= levelModel.CashGoal)
        .OnTrue(speechNodeHasMoney)
        .OnFalse(speechNodeHasNoMoney)
        .After(speechNodeConductorSaysHi)
        .Init(speechBuyTicket);
        var speechNodeMoveToTrain = new SpeechNodeDo(() =>
        {
            levelModel.SetLevelState(LevelState.TRAIN);
            playerModel.InventoryModel.TakeCash(levelModel.CashGoal);
        }).After(speechNodeHasMoney).Init(speechBuyTicket);
        speechBuyTicket.SetRootNode(speechNodeConductorSaysHi);
        return speechBuyTicket;
    }

    private static Speech GetGenericPassengerSpeech()
    {
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
        return speechPassengerGeneric;
    }

    private static Speech GetComradeSpeech(Speech speechPassengerGeneric)
    {
        var speechPassengerComrade = new Speech(2);
        var speechNodeComradeText = new SpeechNodeShowTextLine("Я вас категорически приветствую!").Init(speechPassengerGeneric);
        speechPassengerComrade.SetRootNode(speechNodeComradeText);
        return speechPassengerComrade;
    }

    private static Speech GetAnecdoteSpeech()
    {
        var speechPassengerAnecdote = new Speech(3);
        var speechNodeAnecdoteText1 = new SpeechNodeShowTextLine("Заходит пациент к доктору и говорит").Init(speechPassengerAnecdote);
        var speechNodeAnecdoteText2 = new SpeechNodeShowTextLine("Доктор, у меня член чешется").After(speechNodeAnecdoteText1).Init(speechPassengerAnecdote);
        var speechNodeAnecdoteText3 = new SpeechNodeShowTextLine("Доктор отвечает \"Мой чаще\"").After(speechNodeAnecdoteText2).Init(speechPassengerAnecdote);
        var speechNodeAnecdoteText4 = new SpeechNodeShowTextLine("А пациент такой \"Нет, мой\"").After(speechNodeAnecdoteText3).Init(speechPassengerAnecdote);
        var speechNodeAnecdoteText5 = new SpeechNodeShowTextLine("Ха-ха-ха-ха!").After(speechNodeAnecdoteText4).Init(speechPassengerAnecdote);
        speechPassengerAnecdote.SetRootNode(speechNodeAnecdoteText1);
        return speechPassengerAnecdote;
    }

    private static Speech GetShopkeeperSpeech(PlayerModel playerModel)
    {
        var speechShopkeeper = new Speech(4);

        var speechNodeRich = new SpeechNodeShowTextLineRandom(
            "О, барин прикатил на своей тачке? Бери всё, что хочешь, только монетами не шурши",
            "Для таких, как вы, у меня шампанское по цене самолёта. Не разоришься?",
            "Ваше сиятельство, пачку сигарет? Или предпочитаете золотые с бриллиантами?",
            "Богатенький, налейте мне тоже из вашего кошелька, а то я тут на копейках сижу",
            "Миллионер, не множьтесь тут, платите и валите в свой особняк"
        )
        .Init(speechShopkeeper);

        var speechNodePoor = new SpeechNodeShowTextLineRandom(
            "Опять ты, нищеброд? Хватит на копейки клянчить, иди работай",
            "Бомжара, даже на хлеб не наскрёб? Вали отсюда, не порти воздух",
            "Пять рублей? С таким бюджетом только в помойке копайся, лузер",
            "Ещё один голодранец. Бесплатно не даём, вали просить милостыню",
            "Мой бедолага, ты хоть раз в зеркало смотрел? Плати или проваливай"
        )
        .Init(speechShopkeeper);

        var speechNodeRichCondition = new SpeechNodeConditional(() => playerModel.InventoryModel.CashCount >= 500)
        .OnTrue(speechNodeRich)
        .OnFalse(speechNodePoor)
        .Init(speechShopkeeper);

        speechShopkeeper.SetRootNode(speechNodeRichCondition);
        return speechShopkeeper;
    }
}

using PlatformCore.Services.UI;

public static class SpeechFactory
{
    public static SpeechController GetSpeechController(
        IUIService uiService,
        PlayerModel playerModel,
        PlayerView playerView,
        LevelModel levelModel)
    {
        var speechBuyTicket = GetConductorSpeech(playerModel, levelModel);
        var speechPassengerGeneric = GetGenericPassengerSpeech();
        var speechPassengerComrade = GetComradeSpeech();
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

    private static SpeechNodeParallel Say(Speech speech, string text)
    {
        var parallel = new SpeechNodeParallel();
        parallel.Init(speech);

        var showText = new SpeechNodeShowTextLine(text).Init(speech);
        var playVoice = new SpeechNodePlayVoice().Init(speech);

        parallel.Add(showText, playVoice);
        return parallel;
    }

    private static SpeechNodeParallel SayRandom(Speech speech, params string[] lines)
    {
        var parallel = new SpeechNodeParallel();
        parallel.Init(speech);

        var showText = new SpeechNodeShowTextLineRandom(lines).Init(speech);
        var playVoice = new SpeechNodePlayVoice().Init(speech);

        parallel.Add(showText, playVoice);
        return parallel;
    }

    private static Speech GetConductorSpeech(PlayerModel playerModel, LevelModel levelModel)
    {
        var speechBuyTicket = new Speech(0);

        var speechNodeConductorSaysHi = Say(speechBuyTicket, "Так-так");
        var speechNodeHasMoney = Say(speechBuyTicket, "Добро пожаловать на борт D6-Express.");
        var speechNodeHasNoMoney = Say(speechBuyTicket, "Проваливай, нищий обрыган!");

        var speechNodeConditional = new SpeechNodeConditional(
                () => playerModel.InventoryModel.CashCount >= levelModel.CashGoal)
            .OnTrue(speechNodeHasMoney)
            .OnFalse(speechNodeHasNoMoney)
            .After(speechNodeConductorSaysHi)
            .Init(speechBuyTicket);

        var speechNodeMoveToTrain = new SpeechNodeDo(() =>
            {
                levelModel.SetLevelState(LevelState.TRAIN);
                playerModel.InventoryModel.TakeCash(levelModel.CashGoal);
            })
            .After(speechNodeHasMoney)
            .Init(speechBuyTicket);

        speechBuyTicket.SetRootNode(speechNodeConductorSaysHi);
        return speechBuyTicket;
    }

    private static Speech GetGenericPassengerSpeech()
    {
        var speechPassengerGeneric = new Speech(1);

        // Single node that does (random text + voice) in parallel:
        var root = SayRandom(
            speechPassengerGeneric,
            "Какой сегодня прекрасный день для путешествия!",
            "Надеюсь, поездка будет комфортной и приятной.",
            "Интересно, какие приключения ждут меня впереди?",
            "Я всегда мечтал посетить новые места на поезде.",
            "Люблю звук колес по рельсам, он такой успокаивающий.",
            "Кто-то украл твой сладкий рулетик?"
        );

        speechPassengerGeneric.SetRootNode(root);
        return speechPassengerGeneric;
    }

    private static Speech GetComradeSpeech()
    {
        var speechPassengerComrade = new Speech(2);

        var root = Say(speechPassengerComrade, "Я вас категорически приветствую!");
        speechPassengerComrade.SetRootNode(root);

        return speechPassengerComrade;
    }

    private static Speech GetAnecdoteSpeech()
    {
        var speechPassengerAnecdote = new Speech(3);

        var n1 = Say(speechPassengerAnecdote, "Заходит пациент к доктору и говорит");
        var n2 = Say(speechPassengerAnecdote, "Доктор, у меня член чешется").After(n1);
        var n3 = Say(speechPassengerAnecdote, "Доктор отвечает \"Мой чаще\"").After(n2);
        var n4 = Say(speechPassengerAnecdote, "А пациент такой \"Нет, мой\"").After(n3);
        var n5 = Say(speechPassengerAnecdote, "Ха-ха-ха-ха!").After(n4);

        speechPassengerAnecdote.SetRootNode(n1);
        return speechPassengerAnecdote;
    }

    private static Speech GetShopkeeperSpeech(PlayerModel playerModel)
    {
        var speechShopkeeper = new Speech(4);

        var speechNodeRich = SayRandom(
            speechShopkeeper,
            "О, барин прикатил на своей тачке? Бери всё, что хочешь, только монетами не шурши",
            "Для таких, как вы, у меня шампанское по цене самолёта. Не разоришься?",
            "Ваше сиятельство, пачку сигарет? Или предпочитаете золотые с бриллиантами?",
            "Богатенький, налейте мне тоже из вашего кошелька, а то я тут на копейках сижу",
            "Миллионер, не множьтесь тут, платите и валите в свой особняк"
        );

        var speechNodePoor = SayRandom(
            speechShopkeeper,
            "Опять ты, нищеброд? Хватит на копейки клянчить, иди работай",
            "Бомжара, даже на хлеб не наскрёб? Вали отсюда, не порти воздух",
            "Пять рублей? С таким бюджетом только в помойке копайся, лузер",
            "Ещё один голодранец. Бесплатно не даём, вали просить милостыню",
            "Мой бедолага, ты хоть раз в зеркало смотрел? Плати или проваливай"
        );

        var speechNodeRichCondition = new SpeechNodeConditional(() => playerModel.InventoryModel.CashCount >= 500)
            .OnTrue(speechNodeRich)
            .OnFalse(speechNodePoor)
            .Init(speechShopkeeper);

        speechShopkeeper.SetRootNode(speechNodeRichCondition);
        return speechShopkeeper;
    }
}

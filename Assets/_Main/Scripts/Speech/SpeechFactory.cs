using System.Threading.Tasks;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;

public static class SpeechFactory
{
    public static async Task<SpeechController> GetSpeechController(
        IUIService uiService,
        PlayerModel playerModel,
        PlayerView playerView,
        RunModel runModel,
        ConfigService configService)
    {
        var textsConfig = await configService.GetFirstOrDefaultAsync<TextsConfig>(ResourcePaths.Json.texts_eng);

        var speechBuyTicket = GetConductorSpeech(playerModel, runModel, textsConfig);
        var speechPassengerGeneric = GetGenericPassengerSpeech(textsConfig);
        var speechPassengerComrade = GetComradeSpeech(textsConfig);
        var speechPassengerAnecdote = GetAnecdoteSpeech(textsConfig);
        var speechShopkeeper = GetShopkeeperSpeech(playerModel, textsConfig);

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

    private static Speech GetConductorSpeech(PlayerModel playerModel, RunModel runModel, TextsConfig textsConfig)
    {
        var speechBuyTicket = new Speech(0);

        var speechNodeConductorSaysHi = Say(speechBuyTicket, textsConfig.texts["conductor_enter_hi"]);
        var speechNodeHasMoney = Say(speechBuyTicket, textsConfig.texts["conductor_enter_positive"]);
        var speechNodeHasNoMoney = Say(speechBuyTicket, textsConfig.texts["conductor_enter_negative"]);

        var speechNodeConditional = new SpeechNodeConditional(
                () => playerModel.InventoryModel.CashCount >= runModel.LevelModel.TicketPrice)
            .OnTrue(speechNodeHasMoney)
            .OnFalse(speechNodeHasNoMoney)
            .After(speechNodeConductorSaysHi)
            .Init(speechBuyTicket);

        var speechNodeMoveToTrain = new SpeechNodeDo(() =>
            {
                runModel.SetLevelState(LevelState.TRAIN);
                playerModel.InventoryModel.TakeCash(runModel.LevelModel.TicketPrice);
            })
            .After(speechNodeHasMoney)
            .Init(speechBuyTicket);

        speechBuyTicket.SetRootNode(speechNodeConductorSaysHi);
        return speechBuyTicket;
    }

    private static Speech GetGenericPassengerSpeech(TextsConfig textsConfig)
    {
        var speechPassengerGeneric = new Speech(1);

        // Single node that does (random text + voice) in parallel:
        var root = SayRandom(
            speechPassengerGeneric,
            textsConfig.texts["passenger_random_0"],
            textsConfig.texts["passenger_random_1"],
            textsConfig.texts["passenger_random_2"],
            textsConfig.texts["passenger_random_3"],
            textsConfig.texts["passenger_random_4"],
            textsConfig.texts["passenger_random_5"]
        );

        speechPassengerGeneric.SetRootNode(root);
        return speechPassengerGeneric;
    }

    private static Speech GetComradeSpeech(TextsConfig textsConfig)
    {
        var speechPassengerComrade = new Speech(2);

        var root = Say(speechPassengerComrade, textsConfig.texts["comrade_passenger"]);
        speechPassengerComrade.SetRootNode(root);

        return speechPassengerComrade;
    }

    private static Speech GetAnecdoteSpeech(TextsConfig textsConfig)
    {
        var speechPassengerAnecdote = new Speech(3);

        var n1 = Say(speechPassengerAnecdote, textsConfig.texts["anecdote_0_0"]);
        var n2 = Say(speechPassengerAnecdote, textsConfig.texts["anecdote_0_1"]).After(n1);
        var n3 = Say(speechPassengerAnecdote, textsConfig.texts["anecdote_0_2"]).After(n2);
        var n4 = Say(speechPassengerAnecdote, textsConfig.texts["anecdote_0_3"]).After(n3);
        var n5 = Say(speechPassengerAnecdote, textsConfig.texts["anecdote_0_4"]).After(n4);

        speechPassengerAnecdote.SetRootNode(n1);
        return speechPassengerAnecdote;
    }

    private static Speech GetShopkeeperSpeech(PlayerModel playerModel, TextsConfig textsConfig)
    {
        var speechShopkeeper = new Speech(4);

        var speechNodeRich = SayRandom(
            speechShopkeeper,
            textsConfig.texts["shopkeeper_rich_random_0"],
            textsConfig.texts["shopkeeper_rich_random_1"],
            textsConfig.texts["shopkeeper_rich_random_2"],
            textsConfig.texts["shopkeeper_rich_random_3"],
            textsConfig.texts["shopkeeper_rich_random_4"]
        );

        var speechNodePoor = SayRandom(
            speechShopkeeper,
            textsConfig.texts["shopkeeper_poor_random_0"],
            textsConfig.texts["shopkeeper_poor_random_1"],
            textsConfig.texts["shopkeeper_poor_random_2"],
            textsConfig.texts["shopkeeper_poor_random_3"],
            textsConfig.texts["shopkeeper_poor_random_4"]
        );

        var speechNodeRichCondition = new SpeechNodeConditional(() => playerModel.InventoryModel.CashCount >= 500)
            .OnTrue(speechNodeRich)
            .OnFalse(speechNodePoor)
            .Init(speechShopkeeper);

        speechShopkeeper.SetRootNode(speechNodeRichCondition);
        return speechShopkeeper;
    }
}

using System.Threading.Tasks;
using _Main.Scripts.Core.Services;
using _Main.Scripts.Dice;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;

public static class SpeechFactory
{
    public static async Task<SpeechController> GetSpeechController(
        IUIService uiService,
        PlayerModel playerModel,
        PlayerView playerView,
        D6Game game,
        Run run,
        ConfigService configService,
        IInputService inputService,
        DiceGameModel diceGameModel)
    {
        var textsConfig = await configService.GetFirstOrDefaultAsync<TextsConfig>(ResourcePaths.Json.texts_eng);

        var speechBuyTicket = GetConductorSpeech(playerModel, game, run, textsConfig);
        var speechPassengerGeneric = GetGenericPassengerSpeech(textsConfig);
        var speechPassengerComrade = GetComradeSpeech(textsConfig);
        var speechPassengerAnecdote = GetAnecdoteSpeech(textsConfig);
        var speechShopkeeper = GetShopkeeperSpeech(playerModel, textsConfig);
        var speechShopkeeperFailedBuy = GetShopkeeperFailedBuySpeech(textsConfig);
        var speechEnemy = GetEnemySpeech(run, textsConfig);
        var speechOpponentExit = GetDiceGameOpponentLeaveSpeech(playerView, diceGameModel, textsConfig);
        var speechOpponentNoMoney = GetOpponentNoMoneySpeech(textsConfig);
        var speechOpponentWin = GetOpponentWinSpeech(textsConfig);
        var speechOpponentLose = GetOpponentLooseSpeech(textsConfig);

        var speeches = new Speech[]
        {
            speechBuyTicket,
            speechPassengerGeneric,
            speechPassengerComrade,
            speechPassengerAnecdote,
            speechShopkeeper,
            speechShopkeeperFailedBuy,
            speechEnemy,
            speechOpponentExit,
            speechOpponentNoMoney,
            speechOpponentWin,
            speechOpponentLose
        };

        var speechModel = new SpeechModel(speeches);
        var interactor = playerView.GetComponent<Interactor>();

        return new SpeechController(uiService, interactor, speechModel, inputService);
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

    private static Speech GetConductorSpeech(PlayerModel playerModel, D6Game game, Run run, TextsConfig textsConfig)
    {
        var speechBuyTicket = new Speech(0);

        var speechNodeConductorSaysHi = Say(speechBuyTicket, textsConfig.texts["conductor_enter_hi"]);
        var speechNodeHasMoney = Say(speechBuyTicket, textsConfig.texts["conductor_enter_positive"]);

        var speechNodeHasNoMoney = Say(speechBuyTicket, textsConfig.texts["conductor_enter_negative"]);

        var speechNodeConditional = new SpeechNodeConditional(() => playerModel.InventoryModel.CashCount >= run.TicketPrice)
        .OnTrue(speechNodeHasMoney)
        .OnFalse(speechNodeHasNoMoney)
        .After(speechNodeConductorSaysHi)
        .Init(speechBuyTicket);

        var speechNodeConductorSaysHint = Say(speechBuyTicket, textsConfig.texts["conductor_enter_hint"]);

        var speechNodeMoveToTrain = new SpeechNodeDo(() =>
        {
            game.RequestSetLocation(Location.TRAIN);
            playerModel.InventoryModel.TakeCash(run.TicketPrice);
        })
        .After(speechNodeConductorSaysHint)
        .Init(speechBuyTicket);

        var speechNodeShouldHint = new SpeechNodeConditional(() => run.Level == 0 && run.Day == 0)
        .OnTrue(speechNodeConductorSaysHint)
        .OnFalse(speechNodeMoveToTrain)
        .Init(speechBuyTicket);

        var speechNodeChoiceEnter = new SpeechNodeChoice(textsConfig.texts["choice_enter_train"])
        .OnAccepted(speechNodeShouldHint)
        .After(speechNodeHasMoney)
        .Init(speechBuyTicket);

        speechBuyTicket.SetRootNode(speechNodeConductorSaysHi);
        return speechBuyTicket;
    }

    private static Speech GetDiceGameOpponentLeaveSpeech(PlayerView playerView, DiceGameModel diceGameModel, TextsConfig textsConfig)
    {
        var speechLeaveGame = new Speech(96);

        var speechNodeLeave = new SpeechNodeDo(() =>
        {
            playerView.Interactor.TryStopAction<InteractableActionDiceGame>();
        })
        .Init(speechLeaveGame);

        var tryLeaveOnGameStarted = new SpeechNodeChoice(textsConfig.texts["dice_game_opponent_on_leave"])
        .OnAccepted(speechNodeLeave)
        .Init(speechLeaveGame);

        var tryLeaveOnGameNotStarted = new SpeechNodeChoice(textsConfig.texts["dice_game_opponent_on_early_leave"])
        .OnAccepted(speechNodeLeave)
        .Init(speechLeaveGame);

        var speechNodeConditional = new SpeechNodeConditional(() => diceGameModel.IsDiceGameStarted)
        .OnTrue(tryLeaveOnGameStarted)
        .OnFalse(tryLeaveOnGameNotStarted)
        .Init(speechLeaveGame);

        speechLeaveGame.SetRootNode(speechNodeConditional);
        return speechLeaveGame;
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

    private static Speech GetShopkeeperFailedBuySpeech(TextsConfig textsConfig)
    {
        var speechShopkeeper = new Speech(69);

        var root = SayRandom(
            speechShopkeeper,
            textsConfig.texts["shopkeeper_buy_failed_random_0"]
        );

        speechShopkeeper.SetRootNode(root);
        return speechShopkeeper;
    }

    private static Speech GetEnemySpeech(Run run, TextsConfig textsConfig)
    {
        var speechEnemy = new Speech(123);

        var speechNodePositive = SayRandom(
            speechEnemy,
            textsConfig.texts["enemy_offer"]
        );

        var speechNodeNegative = SayRandom(
            speechEnemy,
            textsConfig.texts["enemy_refuse"]
        );

        var speechNodeConditionTicks = new SpeechNodeConditional(() => run.Tick < run.TicksPerDay)
            .OnTrue(speechNodePositive)
            .OnFalse(speechNodeNegative)
            .Init(speechEnemy);

        speechEnemy.SetRootNode(speechNodeConditionTicks);
        return speechEnemy;
    }

    private static Speech GetOpponentNoMoneySpeech(TextsConfig textsConfig)
    {
        var speechEnemy = new Speech(97);

        var speechNodePositive = Say(
            speechEnemy,
            textsConfig.texts["dice_game_opponent_no_money"]
        );

        speechEnemy.SetRootNode(speechNodePositive);
        return speechEnemy;
    }

    private static Speech GetOpponentWinSpeech(TextsConfig textsConfig)
    {
        var speechEnemy = new Speech(98);

        var speechNodePositive = SayRandom(
            speechEnemy,
            textsConfig.texts["dice_game_opponent_win_1"],
            textsConfig.texts["dice_game_opponent_win_2"],
            textsConfig.texts["dice_game_opponent_win_3"],
            textsConfig.texts["dice_game_opponent_win_4"],
            textsConfig.texts["dice_game_opponent_win_5"]
        );

        speechEnemy.SetRootNode(speechNodePositive);
        return speechEnemy;
    }

    private static Speech GetOpponentLooseSpeech(TextsConfig textsConfig)
    {
        var speechEnemy = new Speech(99);

        var speechNodePositive = SayRandom(
            speechEnemy,
            textsConfig.texts["dice_game_opponent_lose_1"],
            textsConfig.texts["dice_game_opponent_lose_2"],
            textsConfig.texts["dice_game_opponent_lose_3"],
            textsConfig.texts["dice_game_opponent_lose_4"],
            textsConfig.texts["dice_game_opponent_lose_5"]
        );

        speechEnemy.SetRootNode(speechNodePositive);
        return speechEnemy;
    }
}

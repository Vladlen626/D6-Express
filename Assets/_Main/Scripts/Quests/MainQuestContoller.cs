using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;

public class MainQuestContoller : IBaseController, IPreloadable, IQuestGenerator
{
    private readonly Run run;
    private readonly PlayerModel playerModel;
    private readonly ConfigService configService;

    private TextsConfig textsConfig;

    public MainQuestContoller(Run run, PlayerModel playerModel, ConfigService configService)
    {
        this.run = run;
        this.playerModel = playerModel;
        this.configService = configService;
    }

    public void Deactivate() { }

    public async UniTask PreloadAsync()
    {
        textsConfig = await configService.GetFirstOrDefaultAsync<TextsConfig>(ResourcePaths.Json.texts_eng);
    }

    public Quest Generate()
    {
        var quest = new Quest(textsConfig.texts["quest_main_title"]);

        var objectives = quest.AddComponent<QuestComponentObjectives>();

        var getMoneyObjective = objectives.Add();
        var passStationObjective = objectives.Add();

        run.LevelChangeRequested += () =>
        {
            var canMoveNextLevel = playerModel.InventoryModel.CashCount >= run.NextTicketPrice;
            if (canMoveNextLevel)
            {
                quest.RequestComplete();
            }
            else
            {
                quest.RequestFail();
            }
        };

        void UpdateState()
        {
            var canMoveNextLevel = playerModel.InventoryModel.CashCount >= run.NextTicketPrice;
            if (canMoveNextLevel)
            {
                quest.RequestComplete();
            }
            else
            {
                quest.RequestInProgress();
            }
        }

        run.NextTicketPriceChanged += () =>
        {
            UpdateState();
        };

        playerModel.InventoryModel.OnCashCountChanged += () =>
        {
            UpdateState();
        };

        run.LevelChanged += () =>
        {
            UpdateState();
        };

        quest.StateChanged += (q, s) =>
        {
            getMoneyObjective.Completed = s switch
            {
                Quest.State.COMPLETED => true,
                _ => false,
            };
            var localizedMoneyGoal = textsConfig.texts["quest_money_goal"];
            var resultMoneyGoal = string.Format(localizedMoneyGoal, playerModel.InventoryModel.CashCount, run.NextTicketPrice);
            getMoneyObjective.Title = resultMoneyGoal;

            var localizedStationsGoal = textsConfig.texts["quest_stations_goal"];
            var resultStationsGoal = string.Format(localizedStationsGoal, run.LevelsCount - run.Level);
            passStationObjective.Title = resultStationsGoal;
        };

        var localizedMoneyGoal = textsConfig.texts["quest_money_goal"];
        var resultMoneyGoal = string.Format(localizedMoneyGoal, playerModel.InventoryModel.CashCount, run.NextTicketPrice);
        getMoneyObjective.Title = resultMoneyGoal;

        var localizedStationsGoal = textsConfig.texts["quest_stations_goal"];
        var resultStationsGoal = string.Format(localizedStationsGoal, run.LevelsCount - run.Level);
        passStationObjective.Title = resultStationsGoal;

        return quest;
    }
}
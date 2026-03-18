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
    private D6Game d6Game;

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
        var isSubscribed = false;

        bool CanMoveNextLevel()
        {
            return playerModel.InventoryModel.CashCount >= run.NextTicketPrice;
        }

        void UpdateObjectivesText()
        {
            var localizedMoneyGoal = textsConfig.texts["quest_money_goal"];
            var resultMoneyGoal = string.Format(localizedMoneyGoal, playerModel.InventoryModel.CashCount, run.NextTicketPrice);
            getMoneyObjective.Title = resultMoneyGoal;

            var localizedStationsGoal = textsConfig.texts["quest_stations_goal"];
            var resultStationsGoal = string.Format(localizedStationsGoal, run.LevelsCount - run.Level);
            passStationObjective.Title = resultStationsGoal;
        }

        void OnLevelChangeRequested()
        {
            if (CanMoveNextLevel())
            {
                quest.RequestComplete();
            }
            else
            {
                quest.RequestFail();
            }
        }

        void UpdateState()
        {
            if (CanMoveNextLevel())
            {
                quest.RequestComplete();
            }
            else
            {
                quest.RequestInProgress();
            }
        }

        void OnNextTicketPriceChanged()
        {
            UpdateState();
        }

        void OnCashCountChanged()
        {
            UpdateState();
        }

        void OnLevelChanged()
        {
            UpdateState();
        }

        void OnQuestStateChanged(Quest q, Quest.State state)
        {
            getMoneyObjective.Completed = state switch
            {
                Quest.State.COMPLETED => true,
                _ => false,
            };
            UpdateObjectivesText();

            if (state == Quest.State.FINISHED)
            {
                Unsubscribe();
            }
        }

        void Subscribe()
        {
            if (isSubscribed)
            {
                return;
            }

            run.LevelChangeRequested += OnLevelChangeRequested;
            run.NextTicketPriceChanged += OnNextTicketPriceChanged;
            playerModel.InventoryModel.OnCashCountChanged += OnCashCountChanged;
            run.LevelChanged += OnLevelChanged;
            quest.StateChanged += OnQuestStateChanged;
            isSubscribed = true;
        }

        void Unsubscribe()
        {
            if (!isSubscribed)
            {
                return;
            }

            run.LevelChangeRequested -= OnLevelChangeRequested;
            run.NextTicketPriceChanged -= OnNextTicketPriceChanged;
            playerModel.InventoryModel.OnCashCountChanged -= OnCashCountChanged;
            run.LevelChanged -= OnLevelChanged;
            quest.StateChanged -= OnQuestStateChanged;
            isSubscribed = false;
        }

        Subscribe();
        UpdateObjectivesText();

        return quest;
    }
}
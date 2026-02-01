using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;

public class MainQuestContoller : IBaseController, IActivatable, IPreloadable
{
    private readonly Quests quests;
    private readonly Run run;
    private readonly PlayerModel playerModel;
    private readonly ConfigService configService;

    private Quest main;
    private TextsConfig textsConfig;

    public MainQuestContoller(Run run, PlayerModel playerModel, ConfigService configService)
    {
        quests = playerModel.Quests;
        this.run = run;
        this.playerModel = playerModel;
        this.configService = configService;
    }

    public void Activate()
    {
        run.LevelChanged += OnLevelChanged;
        OnLevelChanged();
    }

    public void Deactivate()
    {
        if (main != null)
        {
            main.RequestFinish();
            quests.Remove(main);
        }
    }

    public async UniTask PreloadAsync()
    {
        textsConfig = await configService.GetFirstOrDefaultAsync<TextsConfig>(ResourcePaths.Json.texts_eng);
    }

    private void OnLevelChanged()
    {
        if (main != null)
        {
            main.RequestFinish();
            quests.Remove(main);
        }
        main = CreateMainQuest();
        quests.Add(main);
        main.RequestInProgress();
    }

    private Quest CreateMainQuest()
    {
        var quest = new Quest(textsConfig.texts["quest_main_title"]);

        var hintComponent = quest.AddComponent<QuestComponentHint>();

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

        quest.StateChanged += (q, s) =>
        {
            switch (s)
            {
                case Quest.State.COMPLETED:
                    hintComponent.Mark(true);
                    break;
                default:
                    hintComponent.Mark(false);
                    break;
            }
            var localized = textsConfig.texts["quest_main_goal"];
            var result = string.Format(localized, playerModel.InventoryModel.CashCount, run.NextTicketPrice);
            hintComponent.SetHint(result);
        };

        return quest;
    }
}
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
            getMoneyObjective.Completed = s switch
            {
                Quest.State.COMPLETED => true,
                _ => false,
            };
            var localized = textsConfig.texts["quest_main_goal"];
            var result = string.Format(localized, playerModel.InventoryModel.CashCount, run.NextTicketPrice);
            getMoneyObjective.Title = result;
        };

        var localized = textsConfig.texts["quest_main_goal"];
        var result = string.Format(localized, playerModel.InventoryModel.CashCount, run.NextTicketPrice);
        getMoneyObjective.Title = result;

        return quest;
    }
}
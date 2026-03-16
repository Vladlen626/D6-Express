using System.Threading.Tasks;
using _Main.Scripts.Core.Services;
using _Main.Scripts.Dice;
using PlatformCore.Services.UI;

public static class DebugFactory
{
    public static async Task<DebugMenuUIController> GetBaseController(
        IInputService inputService,
        ICursorService cursorService,
        D6Game game,
        Run run,
        PlayerModel playerModel,
        PlayerView playerView,
        ConfigService configService,
        GlobalNotificationService notificationService,
        DiceGameModel diceGameModel)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        var incrementTick = new DbgMenuItemIncrementTicks(run);
        var incrementDay = new DbgMenuItemIncrementDays(run);
        var switchToStation = new DbgMenuItemSwitchToStation(game);
        var switchToTrain = new DbgMenuItemSwitchToTrain(game);
        var openPlayerWindow = new DbgMenuItemOpenPlayerWindow(run, playerModel, playerView, configService, notificationService, diceGameModel);

        var gameMenu = new DebugMenuModel("Game", incrementTick, incrementDay, switchToStation, switchToTrain, openPlayerWindow);

        await gameMenu.Preload();

        var menu = new DebugMenuUIModel(gameMenu);

        return new DebugMenuUIController(inputService, cursorService, menu);
#else
        await Task.CompletedTask;
        return new DebugMenuUIController(inputService, cursorService, null);
#endif
    }
}

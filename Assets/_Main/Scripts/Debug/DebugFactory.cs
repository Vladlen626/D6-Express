using System.Threading.Tasks;
using _Main.Scripts.Core.Services;
using PlatformCore.Services.UI;

public static class DebugFactory
{
    public static async Task<DebugMenuUIController> GetBaseController(
        IInputService inputService,
        ICursorService cursorService,
        RunModel runModel,
        PlayerModel playerModel,
        PlayerView playerView,
        ConfigService configService,
        Notifications notifications)
    {
        var incrementTick = new DbgMenuItemIncrementTicks(runModel);
        var incrementDay = new DbgMenuItemIncrementDays(runModel);
        var sleep = new DbgMenuItemIncrementSleep();
        var wakeUp = new DbgMenuItemIncrementWakeUp();
        var switchToStation = new DbgMenuItemSwitchToStation(runModel);
        var switchToTrain = new DbgMenuItemSwitchToTrain(runModel);
        var openPlayerWindow = new DbgMenuItemOpenPlayerWindow(playerModel, playerView, configService, notifications);

        var gameMenu = new DebugMenuModel("Game", incrementTick, incrementDay, sleep, wakeUp, switchToStation, switchToTrain, openPlayerWindow);

        await gameMenu.Preload();

        var menu = new DebugMenuUIModel(gameMenu);

        return new DebugMenuUIController(inputService, cursorService, menu);
    }
}

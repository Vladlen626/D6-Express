using System.Threading.Tasks;
using _Main.Scripts.Core.Services;
using PlatformCore.Services.UI;

public static class DebugFactory
{
    public static async Task<DebugMenuUIController> GetBaseController(
        IInputService inputService,
        ICursorService cursorService,
        Run run,
        PlayerModel playerModel,
        PlayerView playerView,
        ConfigService configService,
        Notifications notifications)
    {
        var incrementTick = new DbgMenuItemIncrementTicks(run);
        var incrementDay = new DbgMenuItemIncrementDays(run);
        var switchToStation = new DbgMenuItemSwitchToStation(run);
        var switchToTrain = new DbgMenuItemSwitchToTrain(run);
        var openPlayerWindow = new DbgMenuItemOpenPlayerWindow(run, playerModel, playerView, configService, notifications);

        var gameMenu = new DebugMenuModel("Game", incrementTick, incrementDay, switchToStation, switchToTrain, openPlayerWindow);

        await gameMenu.Preload();

        var menu = new DebugMenuUIModel(gameMenu);

        return new DebugMenuUIController(inputService, cursorService, menu);
    }
}

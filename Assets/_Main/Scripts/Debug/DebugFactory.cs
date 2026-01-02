using System.Collections.Generic;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Services.UI;

public static class DebugFactory
{
    public static IEnumerable<IBaseController> GetBaseController(IInputService inputService, ICursorService cursorService, LevelModel levelModel, PlayerModel playerModel, PlayerView playerView)
    {
        var incrementTick = new DbgMenuItemIncrementTicks(levelModel);
        var incrementDay = new DbgMenuItemIncrementDays(levelModel);
        var sleep = new DbgMenuItemIncrementSleep();
        var wakeUp = new DbgMenuItemIncrementWakeUp();
        var switchToStation = new DbgMenuItemSwitchToStation(levelModel);
        var switchToTrain = new DbgMenuItemSwitchToTrain(levelModel);
        var openPlayerWindow = new DbgMenuItemOpenPlayerWindow(playerModel, playerView);
        
        var gameMenu = new DebugMenuModel("Game", incrementTick, incrementDay, sleep, wakeUp, switchToStation, switchToTrain, openPlayerWindow);
        var menu = new DebugMenuUIModel(gameMenu);

        yield return new DebugMenuUIController(inputService, cursorService, menu);
    }
}

using System.Collections.Generic;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Services.UI;

public static class DebugFactory
{
    public static IEnumerable<IBaseController> GetBaseController(IInputService inputService, ICursorService cursorService, RunModel runModel, PlayerModel playerModel, PlayerView playerView)
    {
        var incrementTick = new DbgMenuItemIncrementTicks(runModel);
        var incrementDay = new DbgMenuItemIncrementDays(runModel);
        var sleep = new DbgMenuItemIncrementSleep();
        var wakeUp = new DbgMenuItemIncrementWakeUp();
        var switchToStation = new DbgMenuItemSwitchToStation(runModel);
        var switchToTrain = new DbgMenuItemSwitchToTrain(runModel);
        var openPlayerWindow = new DbgMenuItemOpenPlayerWindow(playerModel, playerView);
        var openVariablesWindow = new DbgMenuItemOpenDebugVariablesWindow(playerModel, playerView);
        
        var gameMenu = new DebugMenuModel("Game", incrementTick, incrementDay, sleep, wakeUp, switchToStation, switchToTrain, openPlayerWindow, openVariablesWindow);
        var menu = new DebugMenuUIModel(gameMenu);

        yield return new DebugMenuUIController(inputService, cursorService, menu);
    }
}

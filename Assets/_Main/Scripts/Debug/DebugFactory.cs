using System.Collections.Generic;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;

public static class DebugFactory
{
    public static IEnumerable<IBaseController> GetBaseController(IInputService inputService, ICursorService cursorService, LevelModel levelModel)
    {
        var incrementTick = new DbgMenuItemIncrementTicks(levelModel);
        var incrementDay = new DbgMenuItemIncrementDays(levelModel);

        var gameMenu = new DebugMenuModel("Game", incrementTick, incrementDay);
        var menu = new DebugMenuUIModel(gameMenu);

        yield return new DebugMenuUIController(inputService, cursorService, menu);
    }
}

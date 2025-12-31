using System.Collections.Generic;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;

public static class DebugFactory
{
    public static IEnumerable<IBaseController> GetDebugBaseController(IInputService inputService, ICursorService cursorService)
    {
        yield return new DebugUIController(inputService, cursorService);
    }
}

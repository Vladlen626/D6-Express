using System.Collections.Generic;
using PlatformCore.Core;
using PlatformCore.Services.UI;

public static class LevelFactory
{
    public static LevelModel CreateLevelModel()
    {
        // TODO: в настройки
        var ticksPerDay = 3;
        int days = 3;

        var levelModel = new LevelModel(ticksPerDay, days);
        return levelModel;
    }

    public static IEnumerable<IBaseController> GetBaseControllers(IUIService uiService, LevelModel levelModel, SceneContext sceneContext)
    {
        yield return new LevelController(uiService, levelModel, sceneContext.Sun);
    }
}

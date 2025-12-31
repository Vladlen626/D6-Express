using System.Collections.Generic;
using PlatformCore.Core;

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

    public static IEnumerable<IBaseController> GetBaseControllers(SceneContext sceneContext, LevelModel levelModel)
    {
        var levelView = sceneContext.LevelView;

        yield return new LevelController(levelModel, levelView);
    }
}

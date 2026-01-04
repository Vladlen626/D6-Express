using System.Collections.Generic;
using _Main.Scripts.Dice;
using PlatformCore.Core;
using PlatformCore.Services.UI;

public static class LevelFactory
{
	public static LevelModel CreateLevelModel()
	{
		// TODO: в настройки
		var ticksPerDay = 3;
		int days = 3;
		int cashGoal = 500;

		var levelModel = new LevelModel(ticksPerDay, days, cashGoal);
		return levelModel;
	}

	public static IEnumerable<IBaseController> GetBaseControllers(SceneContext sceneContext, IUIService uiService,
		LevelModel levelModel, PlayerModel playerModel, DiceGameModel diceGameModel, PlayerView playerView)
	{
		return new IBaseController[]
		{
			new LevelViewController(uiService, levelModel, sceneContext.Sun, sceneContext.TrainBlock,
				sceneContext.StationBlock, playerView, sceneContext.PlayerTrainSpawnPosition,
				sceneContext.PlayerStationSpawnPosition),
			new LevelController(levelModel, playerModel, diceGameModel)
		};
	}

	public static IEnumerable<IBaseController> GetSleepControllers(LevelModel levelModel, PlayerView playerView)
	{
		var sleepView = playerView.GetComponent<SleepView>();
		var interactor = playerView.GetComponent<Interactor>();

		var sleepController = new SleepController(levelModel, sleepView, interactor);
		yield return sleepController;
	}
}
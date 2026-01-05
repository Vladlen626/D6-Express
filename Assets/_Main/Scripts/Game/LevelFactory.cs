using System.Collections.Generic;
using _Main.Scripts.Dice;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;

public static class LevelFactory
{
	public static async UniTask<LevelModel> CreateLevelModel(ConfigService configService)
	{
		var levelConfig = await configService.GetFirstOrDefaultAsync<LevelConfig>(ResourcePaths.Json.level_rules);

		var levelModel = new LevelModel(levelConfig.ticks_per_day, levelConfig.days, levelConfig.cash_goal);
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
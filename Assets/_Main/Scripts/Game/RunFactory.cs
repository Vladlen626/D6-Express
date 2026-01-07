using System.Collections.Generic;
using _Main.Scripts.Dice;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;

public static class RunFactory
{
	public static async UniTask<RunModel> CreateRunModel(ConfigService configService)
	{
		var runConfig = await configService.GetFirstOrDefaultAsync<RunConfig>(ResourcePaths.Json.run_rules);

		var levelData = new LevelData[runConfig.levels.Length];
		for (int i = 0; i < runConfig.levels.Length; i++)
		{
			LevelConfig item = runConfig.levels[i];
			levelData[i] = new LevelData()
			{
				days = item.days,
				ticks = item.ticks_per_day,
				cashGoal = item.cash_goal
			};
		}
		var runModel = new RunModel();
		runModel.UpdateRun(levelData);

		return runModel;
	}

	public static IEnumerable<IBaseController> GetBaseControllers(SceneContext sceneContext, IUIService uiService,
		RunModel runModel, PlayerModel playerModel, DiceGameModel diceGameModel, PlayerView playerView)
	{
		return new IBaseController[]
		{
			new LevelViewController(uiService, playerModel, runModel, sceneContext.Sun, sceneContext.TrainBlock,
				sceneContext.StationBlock),
			new LevelController(runModel, playerModel, diceGameModel,playerView, sceneContext.PlayerTrainSpawnPosition,
				sceneContext.PlayerStationSpawnPosition)
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
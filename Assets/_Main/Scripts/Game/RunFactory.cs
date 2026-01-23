using System.Collections.Generic;
using System.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;

public static class RunFactory
{
	public static async Task<IEnumerable<IBaseController>> GetBaseControllers(SceneContext sceneContext, IUIService uiService,
		Run run, PlayerModel playerModel, ConfigService configService)
	{
		var runConfig = await configService.GetFirstOrDefaultAsync<RunConfig>(ResourcePaths.Json.run_rules);

		return new IBaseController[]
		{
			new LevelViewController(
				uiService,
				playerModel,
				run,
				sceneContext.Sun),
			new LevelController(
				run,
				runConfig,
				playerModel)
		};
	}

	public static IEnumerable<IBaseController> GetSleepControllers(Run run, PlayerView playerView)
	{
		var sleepView = playerView.GetComponent<SleepView>();
		var interactor = playerView.GetComponent<Interactor>();

		var sleepController = new SleepController(run, sleepView, interactor);
		yield return sleepController;
	}
}
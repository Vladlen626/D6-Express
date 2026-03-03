using System.Collections.Generic;
using System.Threading.Tasks;
using _Main.Scripts.Core.Services;
using _Main.Scripts.Dice;
using PlatformCore.Core;
using PlatformCore.Services;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;

public static class RunFactory
{
	public static async Task<IEnumerable<IBaseController>> GetBaseControllers(
		D6Game game,
		Run run,
		PlayerModel playerModel,
		PlayerView playerView,
		ConfigService configService,
		ICameraService cameraService,
		DiceScoringService scoringService)
	{
		var runConfig = await configService.GetFirstOrDefaultAsync<RunConfig>(ResourcePaths.Json.run_rules);

		return new IBaseController[]
		{
			new LevelViewController(
				game,
				playerView,
				cameraService),
			new RunController(
				game,
				run,
				runConfig,
				playerModel,
				scoringService,
				configService)
		};
	}

	public static SleepController GetSleepControllers(Run run, PlayerView playerView)
	{
		var sleepView = playerView.GetComponent<SleepView>();
		var interactor = playerView.GetComponent<Interactor>();

		return new SleepController(run, sleepView, interactor);
	}
}

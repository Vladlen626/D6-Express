using System.Collections.Generic;
using System.Threading.Tasks;
using _Main.Scripts.Core.Services;
using _Main.Scripts.Dice;
using PlatformCore.Core;
using PlatformCore.Services;

public static class RunFactory
{
	public static Task<IEnumerable<IBaseController>> GetBaseControllers(
		D6Game game,
		Run run,
		PlayerModel playerModel,
		PlayerView playerView,
		ConfigService configService,
		ICameraService cameraService,
		DiceScoringService scoringService)
	{
		IEnumerable<IBaseController> controllers = new IBaseController[]
		{
			new LevelViewController(
				game,
				playerView,
				cameraService),
			new RunController(
				game,
				run,
				playerModel,
				scoringService,
				configService)
		};

		return Task.FromResult(controllers);
	}

	public static SleepController GetSleepControllers(Run run, PlayerView playerView)
	{
		var sleepView = playerView.GetComponent<SleepView>();
		var interactor = playerView.GetComponent<Interactor>();

		return new SleepController(run, sleepView, interactor);
	}
}

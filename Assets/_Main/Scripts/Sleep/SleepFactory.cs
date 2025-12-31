using System.Collections.Generic;
using PlatformCore.Core;
using PlatformCore.Services.UI;

public static class SleepFactory
{
	public static IEnumerable<IBaseController> GetBaseControllers(LevelModel levelModel, PlayerView playerView)
	{
		var sleepView = playerView.GetComponent<SleepView>();
		var interactor = playerView.GetComponent<Interactor>();

		var sleepController = new SleepController(levelModel, sleepView, interactor);
		yield return sleepController;
	}
} 

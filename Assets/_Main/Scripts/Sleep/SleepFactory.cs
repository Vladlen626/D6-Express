using System.Collections.Generic;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;

public static class SleepFactory
{
	public static IEnumerable<IBaseController> GetBaseControllers(PlayerView playerView)
	{
		var sleepView = playerView.GetComponent<SleepView>();
		var interactor = playerView.GetComponent<Interactor>();

		var sleepController = new SleepController(sleepView, interactor);
		yield return sleepController;
	}
} 

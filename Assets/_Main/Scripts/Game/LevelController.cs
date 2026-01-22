using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

public class LevelController : IBaseController, IActivatable
{
	private readonly RunModel runModel;
	private readonly PlayerModel playerModel;

	public LevelController(RunModel runModel, PlayerModel playerModel)
	{
		this.runModel = runModel;
		this.playerModel = playerModel;
	}

	public void Activate()
	{
		runModel.LevelModel.OnFinalDay += OnFinalDayHandler;
	}

	public void Deactivate()
	{
		runModel.LevelModel.OnFinalDay -= OnFinalDayHandler;
	}

	private void OnFinalDayHandler()
	{
		var canMoveNext = playerModel.InventoryModel.CashCount >= runModel.LevelModel.CashGoal;
		if (canMoveNext)
		{
			playerModel.InventoryModel.TakeCash(runModel.LevelModel.CashGoal);
		}
		runModel.LevelModel.SetLevelFinished(canMoveNext);
	}
}
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
		runModel.LevelModel.SetLevelFinished(playerModel.InventoryModel.CashCount >= runModel.LevelModel.CashGoal);
	}
}
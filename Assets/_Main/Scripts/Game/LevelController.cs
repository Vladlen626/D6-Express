using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

public class LevelController : IBaseController, IActivatable
{
	private readonly Run run;
	private readonly RunConfig runConfig;
	private readonly PlayerModel playerModel;

	public LevelController(Run run, RunConfig runConfig, PlayerModel playerModel)
	{
		this.run = run;
		this.playerModel = playerModel;
		this.runConfig = runConfig;
	}

	public void Activate()
	{
		run.LocationChangeRequested += OnLocationChangeRequested;
		run.TickChangeRequested += OnTickChangeRequested;
		run.LevelChangeRequested += OnLevelChangeRequested;
		run.DayChangeRequested += OnDayChangeRequested;

		OnLevelChangeRequested();
	}

	public void Deactivate()
	{
		run.DayChangeRequested -= OnDayChangeRequested;
		run.LevelChangeRequested -= OnLevelChangeRequested;
		run.TickChangeRequested -= OnTickChangeRequested;
		run.LocationChangeRequested -= OnLocationChangeRequested;
	}

	private void OnLocationChangeRequested(Location location)
	{
		run.SetLocation(location);
		run.UpdateProgress(Run.ProgressType.LOCATION_CHANGED);
	}

	private void OnTickChangeRequested(int value)
	{
		run.SetTick(value);
		run.UpdateProgress(Run.ProgressType.SESSION_FINISHED);
	}

	private void OnLevelChangeRequested()
	{
		if (run.Level + 1 >= 0)
		{
			var levelData = runConfig.levels[run.Level];

			var ticketPrice = run.Level == 0 ? 0 : runConfig.levels[run.Level - 1].cash_goal;
			run.SetLevelData(levelData.station_id, levelData.days, levelData.ticks_per_day, runConfig.levels.Length, ticketPrice, levelData.cash_goal);
			run.UpdateProgress(Run.ProgressType.LEVEL_FINISHED);
		}
	}

	private void OnDayChangeRequested(int value)
	{
		if (value >= run.DaysPerLevel)
		{
			var canMoveNextLevel = playerModel.InventoryModel.CashCount >= run.NextTicketPrice;
			if (canMoveNextLevel)
			{
				if (run.Level + 1 == run.LevelsCount - 1)
				{
					run.FinishRun(true);
					run.UpdateProgress(Run.ProgressType.WIN);
				}
				else
				{
					run.FinishLevel(true);
					run.SetLocation(Location.STATION);
					OnLevelChangeRequested();
				}
			}
			else
			{
				run.FinishRun(false);
				run.UpdateProgress(Run.ProgressType.LOSE);
			}
		}
		else
		{
			run.SetDay(value);
			run.UpdateProgress(Run.ProgressType.DAY_FINISHED);
		}
	}


}
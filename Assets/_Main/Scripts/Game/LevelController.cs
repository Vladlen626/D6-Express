using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

public class LevelController : IBaseController, IActivatable
{
	private readonly D6Game game;
	private readonly Run run;
	private readonly RunConfig runConfig;
	private readonly PlayerModel playerModel;

	public LevelController(D6Game game, Run run, RunConfig runConfig, PlayerModel playerModel)
	{
		this.game = game;
		this.run = run;
		this.playerModel = playerModel;
		this.runConfig = runConfig;
	}

	public void Activate()
	{
		run.TickChangeRequested += OnTickChangeRequested;
		run.LevelChangeRequested += OnLevelChangeRequested;
		run.DayChangeRequested += OnDayChangeRequested;
		run.RunStarted += UpdateLevelData;
	}

	public void Deactivate()
	{
		run.RunStarted -= UpdateLevelData;
		run.DayChangeRequested -= OnDayChangeRequested;
		run.LevelChangeRequested -= OnLevelChangeRequested;
		run.TickChangeRequested -= OnTickChangeRequested;
	}

	private void OnTickChangeRequested(int value)
	{
		run.SetTick(value);
		game.NotifyTickChanged();
	}

	private void OnLevelChangeRequested()
	{
		UpdateLevelData();
	}

	private void UpdateLevelData()
	{
		var levelData = runConfig.levels[run.Level];
		var ticketPrice = run.Level == 0 ? 0 : runConfig.levels[run.Level - 1].cash_goal;
		run.SetLevelData(levelData.station_id, levelData.days, levelData.ticks_per_day, runConfig.levels.Length, ticketPrice, levelData.cash_goal);
	}

	private void OnDayChangeRequested(int value)
	{
		if (value >= run.DaysPerLevel)
		{
			var canMoveNextLevel = playerModel.InventoryModel.CashCount >= run.NextTicketPrice;
			if (canMoveNextLevel)
			{
				if (run.Level + 1 == run.LevelsCount)
				{
					run.FinishRun(true);
				}
				else
				{
					run.FinishLevel(true);
					game.SetLocation(Location.STATION);
				}
			}
			else
			{
				run.FinishRun(false);
			}
		}
		else
		{
			run.SetDay(value);
			game.NotifyDayChanged();
		}
	}


}
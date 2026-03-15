using _Main.Scripts.Dice;
using System;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;

public class RunController : IBaseController, IActivatable, IPreloadable
{
	private readonly D6Game game;
	private readonly Run run;
	private readonly RunConfig runConfig;
	private readonly PlayerModel playerModel;
	private readonly DiceScoringService scoringService;
	private readonly ConfigService configService;

	private PlayerConfig playerConfig;

	public RunController(D6Game game, Run run, RunConfig runConfig, PlayerModel playerModel, DiceScoringService scoringService, ConfigService configService)
	{
		this.game = game;
		this.run = run;
		this.playerModel = playerModel;
		this.scoringService = scoringService;
		this.configService = configService;
		this.runConfig = runConfig;
	}

	public void Activate()
	{
		run.TickChangeRequested += OnTickChangeRequested;
		run.LevelChangeRequested += OnLevelChangeRequested;
		run.DayChangeRequested += OnDayChangeRequested;
		run.RunStarted += OnRunStarted;
	}

	public void Deactivate()
	{
		run.RunStarted -= OnRunStarted;
		run.DayChangeRequested -= OnDayChangeRequested;
		run.LevelChangeRequested -= OnLevelChangeRequested;
		run.TickChangeRequested -= OnTickChangeRequested;
	}

	public async UniTask PreloadAsync()
	{
		playerConfig = await configService.GetFirstOrDefaultAsync<PlayerConfig>(ResourcePaths.Json.player);
		if (playerConfig == null)
		{
			throw new InvalidOperationException("[RunController] Player config was not loaded.");
		}

		playerModel.InventoryModel.SetModifierItemsCapacity(playerConfig.modifierItemsCapacity);
	}

	private void OnTickChangeRequested(int value)
	{
		if (value >= run.TicksPerDay && !CanMoveNextLevel())
		{
			run.FinishRun(false);
		}
		else
		{
			run.SetTick(value);
			game.NotifyTickChanged();
		}
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

	private void OnRunStarted()
	{
		UpdateLevelData();

		int startCash = playerConfig.cash;
		playerModel.InventoryModel.SetCash(startCash);

		playerModel.InventoryModel.RemoveAllDices();
		if (playerConfig.dices != null)
		{
			foreach (var playerConfigDice in playerConfig.dices)
			{
				playerModel.InventoryModel.AddDice(playerConfigDice);
			}
		}

		playerModel.InventoryModel.RemoveAllModifierItems();
		if (playerConfig.modifierItems != null)
		{
			foreach (var modifierId in playerConfig.modifierItems)
			{
				var addResult = playerModel.InventoryModel.TryAddModifierItem(modifierId);
				if (addResult != ModifierItemAddResult.Success)
				{
					throw new InvalidOperationException(
						$"[RunController] Failed to add starting modifier item '{modifierId}'. Reason: {addResult}.");
				}
			}
		}

		// Initialize straight upgrade state for the new run
		var defaults = scoringService.GetStraightDefaults();
		var straightState = new StraightRuntimeState(defaults);
		scoringService.SetStraightState(straightState);
		run.SetStraightState(straightState);
	}

	private void OnDayChangeRequested(int value)
	{
		if (value >= run.DaysPerLevel)
		{
			if (CanMoveNextLevel())
			{
				if (run.Level + 1 == run.LevelsCount)
				{
					run.FinishRun(true);
				}
				else
				{
					run.FinishLevel(true);
					UpdateLevelData();
					game.RequestSetLocation(Location.STATION);
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

	private bool CanMoveNextLevel()
	{
		return playerModel.InventoryModel.CashCount >= run.NextTicketPrice;
	}
}

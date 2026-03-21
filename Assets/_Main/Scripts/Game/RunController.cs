using _Main.Scripts.Dice;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;

public class RunController : IBaseController, IActivatable, IPreloadable
{
	private const string DiceGameTacticsPoolPath = "Json/dice_game_tactics_pool";

	private readonly D6Game game;
	private readonly Run run;
	private readonly PlayerModel playerModel;
	private readonly DiceScoringService scoringService;
	private readonly ConfigService configService;

	private PlayerConfig playerConfig;
	private DiceGameTacticsPoolConfig diceGameTacticsPoolConfig;
	private Dictionary<string, RunConfig> runConfigs;

	public RunController(D6Game game, Run run, PlayerModel playerModel, DiceScoringService scoringService, ConfigService configService)
	{
		this.game = game;
		this.run = run;
		this.playerModel = playerModel;
		this.scoringService = scoringService;
		this.configService = configService;
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

		diceGameTacticsPoolConfig =
			await configService.GetFirstOrDefaultAsync<DiceGameTacticsPoolConfig>(DiceGameTacticsPoolPath);
		if (diceGameTacticsPoolConfig == null)
		{
			throw new InvalidOperationException($"[RunController] Tactics pool config '{DiceGameTacticsPoolPath}' was not loaded.");
		}

		if (!diceGameTacticsPoolConfig.TryValidateStatic(out var validationError))
		{
			throw new InvalidOperationException($"[RunController] Tactics pool validation failed: {validationError}");
		}

		runConfigs = await configService.GetConfigsAsync<RunConfig>(ResourcePaths.Json.run_rules);
		if (runConfigs == null || runConfigs.Count == 0)
		{
			throw new InvalidOperationException("[RunController] Run rules config is empty.");
		}
	}

	private void OnTickChangeRequested(int value)
	{
		if (value >= run.TicksPerDay && !CanMoveNextLevel())
		{
			run.FinishRun(Run.FinishType.LOSE);
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
		var activeRunConfig = GetActiveRunConfig();
		if (run.Level < 0 || run.Level >= activeRunConfig.levels.Length)
		{
			throw new InvalidOperationException(
				$"[RunController] Level index {run.Level} is out of bounds for run rules '{run.RunRulesId}'.");
		}

		var levelData = activeRunConfig.levels[run.Level];
		var ticketPrice = run.Level == 0 ? 0 : activeRunConfig.levels[run.Level - 1].cash_goal;
		run.SetLevelData(
			levelData.station_id,
			levelData.days,
			levelData.ticks_per_day,
			activeRunConfig.levels.Length,
			ticketPrice,
			levelData.cash_goal);
	}

	private void OnRunStarted()
	{
		SelectRunTactic();
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

		// Reset all combo upgrade runtime states for the new run.
		scoringService.ResetUpgradeStatesToDefaults();
		run.SetStraightState(scoringService.GetStraightState());
	}

	private void SelectRunTactic()
	{
		if (diceGameTacticsPoolConfig?.tactics == null || diceGameTacticsPoolConfig.tactics.Count == 0)
		{
			throw new InvalidOperationException("[RunController] Tactics pool is empty.");
		}

		int totalWeight = 0;
		for (int i = 0; i < diceGameTacticsPoolConfig.tactics.Count; i++)
		{
			totalWeight += diceGameTacticsPoolConfig.tactics[i].weight;
		}

		if (totalWeight <= 0)
		{
			throw new InvalidOperationException("[RunController] Tactics pool total weight must be > 0.");
		}

		unchecked
		{
			var guidHash = Guid.NewGuid().GetHashCode();
			var ticksHash = DateTime.UtcNow.Ticks.GetHashCode();
			var envHash = Environment.TickCount;
			var seed = (guidHash * 397) ^ ticksHash ^ envHash;
			var random = new Random(seed);
			var roll = random.Next(totalWeight);
			int cumulativeWeight = 0;
			DiceGameTacticProfileConfig selectedProfile = null;
			for (int i = 0; i < diceGameTacticsPoolConfig.tactics.Count; i++)
			{
				var profile = diceGameTacticsPoolConfig.tactics[i];
				cumulativeWeight += profile.weight;
				if (roll < cumulativeWeight)
				{
					selectedProfile = profile;
					break;
				}
			}

			if (selectedProfile == null)
			{
				throw new InvalidOperationException("[RunController] Failed to resolve tactic profile.");
			}

			run.SetDiceGameTacticSelection(
				selectedProfile.id,
				selectedProfile.enemy_ai_scenarios_path,
				selectedProfile.enemy_ai_scenario_schedule_path,
				selectedProfile.modifiers_schedule_path);

			UnityEngine.Debug.Log(
				$"[DiceGame][Tactic] Roll={roll}, totalWeight={totalWeight}, tacticsCount={diceGameTacticsPoolConfig.tactics.Count}. " +
				$"seed={seed}. " +
				$"Selected tactic='{selectedProfile.id}' " +
				$"enemy_ai_scenarios_path='{selectedProfile.enemy_ai_scenarios_path}' " +
				$"enemy_ai_scenario_schedule_path='{selectedProfile.enemy_ai_scenario_schedule_path}' " +
				$"modifiers_schedule_path='{selectedProfile.modifiers_schedule_path}'.");
		}
	}

	private void OnDayChangeRequested(int value)
	{
		if (value >= run.DaysPerLevel)
		{
			if (CanMoveNextLevel())
			{
				if (run.Level + 1 == run.LevelsCount)
				{
					run.FinishRun(Run.FinishType.WIN);
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
				run.FinishRun(Run.FinishType.LOSE);
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

	private RunConfig GetActiveRunConfig()
	{
		if (runConfigs == null || runConfigs.Count == 0)
		{
			throw new InvalidOperationException("[RunController] Run rules config is empty.");
		}

		if (!runConfigs.TryGetValue(run.RunRulesId, out var activeRunConfig) || activeRunConfig == null)
		{
			throw new InvalidOperationException(
				$"[RunController] Run rules with id '{run.RunRulesId}' were not found.");
		}

		if (activeRunConfig.levels == null || activeRunConfig.levels.Length == 0)
		{
			throw new InvalidOperationException(
				$"[RunController] Run rules '{run.RunRulesId}' do not contain levels.");
		}

		return activeRunConfig;
	}
}

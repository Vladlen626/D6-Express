using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using PlatformCore.Services;
using PlatformCore.Services.Factory;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public sealed class DiceGameScenarioSetup
	{
		private readonly DiceGameModel diceGameModel;
		private readonly Run run;
		private readonly ConfigService configService;
		private readonly IResourceService resourceService;
		private readonly List<IModifier> runtimeGlobalModifiers = new();

		public EnemyAiScenarioRuntime EnemyScenarioRuntime { get; private set; }

		public DiceGameScenarioSetup(
			DiceGameModel diceGameModel,
			Run run,
			ConfigService configService,
			IResourceService resourceService)
		{
			this.diceGameModel = diceGameModel;
			this.run = run;
			this.configService = configService;
			this.resourceService = resourceService;
		}

		public async UniTask<bool> SetupEnemyAiScenarioAsync(DiceGameConfig diceGameConfig)
		{
			EnemyScenarioRuntime = null;
			var scriptedModeEnabled = string.Equals(
				diceGameConfig.enemy_ai_mode,
				EnemyAiMode.Scripted,
				StringComparison.OrdinalIgnoreCase);

			if (!scriptedModeEnabled)
			{
				return true;
			}

			var scenarioMap = await LoadEnemyScenarioMapAsync();
			if (scenarioMap == null)
			{
				return false;
			}

			var scenarioId = await ResolveScenarioIdAsync(diceGameConfig);
			if (string.IsNullOrWhiteSpace(scenarioId))
			{
				return false;
			}

			if (!scenarioMap.TryGetValue(scenarioId, out var scenario) || scenario == null)
			{
				FailDiceGameSetup($"[DiceGame] Enemy AI scenario '{scenarioId}' not found in map '{ResourcePaths.Json.enemy_ai_scenarios}'.");
				return false;
			}

			if (!scenario.TryValidateStatic(out var validationError))
			{
				FailDiceGameSetup($"[DiceGame] Scenario validation failed: {validationError}");
				return false;
			}

			if (scenario.target_score > 0 && scenario.target_score != diceGameModel.TargetPoints)
			{
				FailDiceGameSetup(
					$"[DiceGame] Scenario target_score ({scenario.target_score}) does not match dice_game_rules target_score ({diceGameModel.TargetPoints}).");
				return false;
			}

			diceGameModel.SetEnemyComboUpgradesEnabled(false);
			EnemyScenarioRuntime = new EnemyAiScenarioRuntime(scenario);
			return true;
		}

		public async UniTask<bool> SetupModifiersAsync(DiceGameConfig diceGameConfig)
		{
			var modifiersMode = diceGameConfig.modifiers_mode?.Trim();
			if (string.IsNullOrWhiteSpace(modifiersMode))
			{
				modifiersMode = DiceGameModifiersMode.Inventory;
			}

			if (string.Equals(modifiersMode, DiceGameModifiersMode.Inventory, StringComparison.OrdinalIgnoreCase))
			{
				return await ApplyInventoryModifiersAsync();
			}

			if (string.Equals(modifiersMode, DiceGameModifiersMode.Scripted, StringComparison.OrdinalIgnoreCase))
			{
				return await ApplyModifiersScheduleAsync(diceGameConfig.modifiers_set_id);
			}

			FailDiceGameSetup(
				$"[DiceGame] Unsupported modifiers_mode '{diceGameConfig.modifiers_mode}'. Expected '{DiceGameModifiersMode.Inventory}' or '{DiceGameModifiersMode.Scripted}'.");
			return false;
		}

		private async UniTask<Dictionary<string, EnemyAiScenarioConfig>> LoadEnemyScenarioMapAsync()
		{
			var textAsset = await resourceService.LoadAsync<TextAsset>(ResourcePaths.Json.enemy_ai_scenarios);
			if (!textAsset)
			{
				FailDiceGameSetup($"[DiceGame] Enemy AI scenarios file '{ResourcePaths.Json.enemy_ai_scenarios}' not found.");
				return null;
			}

			Dictionary<string, EnemyAiScenarioConfig> scenarioMap;
			try
			{
				scenarioMap = JsonConvert.DeserializeObject<Dictionary<string, EnemyAiScenarioConfig>>(textAsset.text);
			}
			catch (Exception exception)
			{
				FailDiceGameSetup($"[DiceGame] Failed to parse scenarios map '{ResourcePaths.Json.enemy_ai_scenarios}': {exception.Message}");
				return null;
			}

			if (scenarioMap == null || scenarioMap.Count == 0)
			{
				FailDiceGameSetup($"[DiceGame] Scenarios map '{ResourcePaths.Json.enemy_ai_scenarios}' is empty.");
				return null;
			}

			foreach (var pair in scenarioMap)
			{
				if (string.IsNullOrWhiteSpace(pair.Key))
				{
					FailDiceGameSetup("[DiceGame] Scenarios map contains an empty key.");
					return null;
				}

				if (pair.Value == null)
				{
					FailDiceGameSetup($"[DiceGame] Scenario '{pair.Key}' is null.");
					return null;
				}

				pair.Value.id = pair.Key;
				pair.Value.ParseConfig();
			}

			return scenarioMap;
		}

		private async UniTask<string> ResolveScenarioIdAsync(DiceGameConfig diceGameConfig)
		{
			var scenarioId = diceGameConfig.enemy_ai_scenario_id?.Trim();
			if (!string.IsNullOrWhiteSpace(scenarioId))
			{
				return scenarioId;
			}

			var schedule = await configService.GetFirstOrDefaultAsync<EnemyAiScenarioScheduleConfig>(ResourcePaths.Json.enemy_ai_scenario_schedule);
			if (schedule == null)
			{
				FailDiceGameSetup($"[DiceGame] Enemy AI schedule '{ResourcePaths.Json.enemy_ai_scenario_schedule}' not found and enemy_ai_scenario_id override is empty.");
				return null;
			}

			schedule.ParseConfig();
			if (!schedule.TryValidateStatic(out var scheduleValidationError))
			{
				FailDiceGameSetup($"[DiceGame] Enemy AI schedule validation failed: {scheduleValidationError}");
				return null;
			}

			// Schedule values are 1-based for easier authoring in JSON.
			var level = run.Level + 1;
			var day = run.Day + 1;
			var match = run.Tick + 1;
			if (!schedule.TryResolveScenarioId(level, day, match, out scenarioId))
			{
				FailDiceGameSetup($"[DiceGame] Enemy AI schedule could not resolve scenario for level={level}, day={day}, match={match}.");
				return null;
			}

			return scenarioId;
		}

		private UniTask<bool> ApplyInventoryModifiersAsync()
		{
			// Keep player's inventory-driven modifiers intact in default mode.
			ClearRuntimeGlobalModifiers();
			diceGameModel.EnemyModifierItemsModel.Reset();
			diceGameModel.EnemyModifiersModel.Reset();
			return UniTask.FromResult(true);
		}

		private async UniTask<bool> ApplyModifiersScheduleAsync(string setIdOverride)
		{
			var schedule = await configService.GetFirstOrDefaultAsync<DiceGameModifiersScheduleConfig>(ResourcePaths.Json.dice_game_modifiers_schedule);
			if (schedule == null)
			{
				FailDiceGameSetup($"[DiceGame] Modifiers schedule '{ResourcePaths.Json.dice_game_modifiers_schedule}' not found.");
				return false;
			}

			schedule.ParseConfig();
			if (!schedule.TryValidateStatic(out var validationError))
			{
				FailDiceGameSetup($"[DiceGame] Modifiers schedule validation failed: {validationError}");
				return false;
			}

			DiceGameModifierSet resolvedSet = null;
			var overrideSetId = setIdOverride?.Trim();
			if (!string.IsNullOrWhiteSpace(overrideSetId))
			{
				if (!schedule.sets.TryGetValue(overrideSetId, out resolvedSet) || resolvedSet == null)
				{
					FailDiceGameSetup(
						$"[DiceGame] modifiers_set_id override '{overrideSetId}' not found in schedule '{ResourcePaths.Json.dice_game_modifiers_schedule}'.");
					return false;
				}
			}
			else
			{
				var level = run.Level + 1;
				var day = run.Day + 1;
				var match = run.Tick + 1;
				if (!schedule.TryResolveSet(level, day, match, out resolvedSet) || resolvedSet == null)
				{
					FailDiceGameSetup($"[DiceGame] Modifiers schedule could not resolve set for level={level}, day={day}, match={match}.");
					return false;
				}
			}

			ClearRuntimeGlobalModifiers();
			diceGameModel.EnemyModifierItemsModel.Reset();
			diceGameModel.EnemyModifiersModel.Reset();

			return ApplyGlobalModifierSet(resolvedSet.player_modifiers, diceGameModel.PlayerScoringService, "player");
		}

		private bool ApplyGlobalModifierSet(
			string[] modifierIds,
			DiceScoringService scoringService,
			string sideLabel)
		{
			if (modifierIds == null || modifierIds.Length == 0)
			{
				return true;
			}

			var uniqueIds = new HashSet<string>();
			for (int i = 0; i < modifierIds.Length; i++)
			{
				var modifierId = modifierIds[i]?.Trim();
				if (string.IsNullOrWhiteSpace(modifierId))
				{
					FailDiceGameSetup($"[DiceGame] Empty modifier id in {sideLabel}_modifiers.");
					return false;
				}

				if (!uniqueIds.Add(modifierId))
				{
					FailDiceGameSetup($"[DiceGame] Duplicate modifier id '{modifierId}' in {sideLabel}_modifiers.");
					return false;
				}

				var modifier = GlobalModifierFactory.Create(modifierId, scoringService);
				if (modifier == null)
				{
					FailDiceGameSetup($"[DiceGame] Failed to create modifier '{modifierId}' for {sideLabel}.");
					return false;
				}

				diceGameModel.PlayerModifiersModel.AddModifier(modifier);
				runtimeGlobalModifiers.Add(modifier);
			}

			return true;
		}

		private void ClearRuntimeGlobalModifiers()
		{
			if (runtimeGlobalModifiers.Count == 0)
			{
				return;
			}

			var model = diceGameModel.PlayerModifiersModel;
			for (int i = 0; i < runtimeGlobalModifiers.Count; i++)
			{
				model.RemoveModifier(runtimeGlobalModifiers[i]);
			}

			runtimeGlobalModifiers.Clear();
		}

		private void FailDiceGameSetup(string message)
		{
			Debug.LogError(message);
			diceGameModel.SetConditionFailed();
		}
	}
}

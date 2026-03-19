using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using PlatformCore.Services;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public sealed class DiceGameScenarioSetup
	{
		private readonly DiceGameModel diceGameModel;
		private readonly Run run;
		private readonly IResourceService resourceService;
		private readonly List<IModifier> runtimeGlobalModifiers = new();

		public EnemyAiScenarioRuntime EnemyScenarioRuntime { get; private set; }

		public DiceGameScenarioSetup(
			DiceGameModel diceGameModel,
			Run run,
			IResourceService resourceService)
		{
			this.diceGameModel = diceGameModel;
			this.run = run;
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

			if (!TryGetRunTacticPaths(out var scenariosPath, out var scenarioSchedulePath, out _))
			{
				return false;
			}

			var scenarioMap = await LoadEnemyScenarioMapAsync(scenariosPath);
			if (scenarioMap == null)
			{
				return false;
			}

			var scenarioId = await ResolveScenarioIdAsync(diceGameConfig, scenarioSchedulePath);
			if (string.IsNullOrWhiteSpace(scenarioId))
			{
				return false;
			}

			if (!scenarioMap.TryGetValue(scenarioId, out var scenario) || scenario == null)
			{
				FailDiceGameSetup($"[DiceGame] Enemy AI scenario '{scenarioId}' not found in map '{scenariosPath}'.");
				return false;
			}

			if (!scenario.TryValidateStatic(out var validationError))
			{
				FailDiceGameSetup($"[DiceGame] Scenario validation failed: {validationError}");
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
				if (!TryGetRunTacticPaths(out _, out _, out var modifiersSchedulePath))
				{
					return false;
				}

				return await ApplyModifiersScheduleAsync(diceGameConfig.modifiers_set_id, modifiersSchedulePath);
			}

			FailDiceGameSetup(
				$"[DiceGame] Unsupported modifiers_mode '{diceGameConfig.modifiers_mode}'. Expected '{DiceGameModifiersMode.Inventory}' or '{DiceGameModifiersMode.Scripted}'.");
			return false;
		}

		private async UniTask<Dictionary<string, EnemyAiScenarioConfig>> LoadEnemyScenarioMapAsync(string scenariosPath)
		{
			var textAsset = await resourceService.LoadAsync<TextAsset>(scenariosPath);
			if (!textAsset)
			{
				FailDiceGameSetup($"[DiceGame] Enemy AI scenarios file '{scenariosPath}' not found.");
				return null;
			}

			Dictionary<string, EnemyAiScenarioConfig> scenarioMap;
			try
			{
				scenarioMap = JsonConvert.DeserializeObject<Dictionary<string, EnemyAiScenarioConfig>>(textAsset.text);
			}
			catch (Exception exception)
			{
				FailDiceGameSetup($"[DiceGame] Failed to parse scenarios map '{scenariosPath}': {exception.Message}");
				return null;
			}

			if (scenarioMap == null || scenarioMap.Count == 0)
			{
				FailDiceGameSetup($"[DiceGame] Scenarios map '{scenariosPath}' is empty.");
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

		private async UniTask<string> ResolveScenarioIdAsync(
			DiceGameConfig diceGameConfig,
			string scenarioSchedulePath)
		{
			var scenarioId = diceGameConfig.enemy_ai_scenario_id?.Trim();
			if (!string.IsNullOrWhiteSpace(scenarioId))
			{
				Debug.Log($"[DiceGame][Scenario] Using override enemy_ai_scenario_id='{scenarioId}'.");
				return scenarioId;
			}

			var schedule =
				await LoadConfigFromJsonAsync<EnemyAiScenarioScheduleConfig>(scenarioSchedulePath, "Enemy AI schedule");
			if (schedule == null)
			{
				return null;
			}

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
				FailDiceGameSetup($"[DiceGame] Enemy AI schedule '{scenarioSchedulePath}' could not resolve scenario for level={level}, day={day}, match={match}.");
				return null;
			}

			Debug.Log(
				$"[DiceGame][Scenario] Resolved scenario_id='{scenarioId}' from schedule='{scenarioSchedulePath}' " +
				$"for level={level}, day={day}, match={match}.");
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

		private async UniTask<bool> ApplyModifiersScheduleAsync(string setIdOverride, string modifiersSchedulePath)
		{
			var schedule = await LoadConfigFromJsonAsync<DiceGameModifiersScheduleConfig>(
				modifiersSchedulePath,
				"modifiers schedule");
			if (schedule == null)
			{
				return false;
			}

			if (!schedule.TryValidateStatic(out var validationError))
			{
				FailDiceGameSetup($"[DiceGame] Modifiers schedule validation failed: {validationError}");
				return false;
			}

			DiceGameModifierSet resolvedSet = null;
			string resolvedSetId = null;
			var overrideSetId = setIdOverride?.Trim();
			if (!string.IsNullOrWhiteSpace(overrideSetId))
			{
				if (!schedule.sets.TryGetValue(overrideSetId, out resolvedSet) || resolvedSet == null)
				{
					FailDiceGameSetup(
						$"[DiceGame] modifiers_set_id override '{overrideSetId}' not found in schedule '{modifiersSchedulePath}'.");
					return false;
				}

				resolvedSetId = overrideSetId;
			}
			else
			{
				var level = run.Level + 1;
				var day = run.Day + 1;
				var match = run.Tick + 1;
				if (!schedule.TryResolveSet(level, day, match, out resolvedSet) || resolvedSet == null)
				{
					FailDiceGameSetup($"[DiceGame] Modifiers schedule '{modifiersSchedulePath}' could not resolve set for level={level}, day={day}, match={match}.");
					return false;
				}

				resolvedSetId = FindSetIdByReference(schedule, resolvedSet);
				Debug.Log(
					$"[DiceGame][Modifiers] Resolved set_id='{resolvedSetId}' from schedule='{modifiersSchedulePath}' " +
					$"for level={level}, day={day}, match={match}.");
			}

			if (!string.IsNullOrWhiteSpace(overrideSetId))
			{
				Debug.Log($"[DiceGame][Modifiers] Using override modifiers_set_id='{resolvedSetId}'.");
			}

			ClearRuntimeGlobalModifiers();
			diceGameModel.EnemyModifierItemsModel.Reset();
			diceGameModel.EnemyModifiersModel.Reset();

			return ApplyGlobalModifierSet(resolvedSet.player_modifiers, diceGameModel.PlayerScoringService, "player");
		}

		private static string FindSetIdByReference(
			DiceGameModifiersScheduleConfig schedule,
			DiceGameModifierSet resolvedSet)
		{
			foreach (var pair in schedule.sets)
			{
				if (ReferenceEquals(pair.Value, resolvedSet))
				{
					return pair.Key;
				}
			}

			return "<unknown>";
		}

		private bool TryGetRunTacticPaths(
			out string scenariosPath,
			out string scenarioSchedulePath,
			out string modifiersSchedulePath)
		{
			scenariosPath = null;
			scenarioSchedulePath = null;
			modifiersSchedulePath = null;

			if (!run.HasDiceGameTacticSelection)
			{
				FailDiceGameSetup("[DiceGame] Run tactic profile is not selected.");
				return false;
			}

			scenariosPath = run.EnemyAiScenariosPath;
			scenarioSchedulePath = run.EnemyAiScenarioSchedulePath;
			modifiersSchedulePath = run.ModifiersSchedulePath;
			return true;
		}

		private async UniTask<TConfig> LoadConfigFromJsonAsync<TConfig>(string path, string label)
			where TConfig : BaseConfig
		{
			var textAsset = await resourceService.LoadAsync<TextAsset>(path);
			if (!textAsset)
			{
				FailDiceGameSetup($"[DiceGame] {label} '{path}' not found.");
				return null;
			}

			TConfig config;
			try
			{
				config = JsonConvert.DeserializeObject<TConfig>(textAsset.text);
			}
			catch (Exception exception)
			{
				FailDiceGameSetup($"[DiceGame] Failed to parse {label} '{path}': {exception.Message}");
				return null;
			}

			if (config == null)
			{
				FailDiceGameSetup($"[DiceGame] Parsed {label} '{path}' is null.");
				return null;
			}

			config.ParseConfig();
			return config;
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
			diceGameModel.SetConditionFailed(
				DiceMatchResultReason.SetupFailed,
				DiceMatchStage.Setup,
				"scenario_setup");
		}
	}
}

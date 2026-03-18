using System;
using System.Collections.Generic;

[Serializable]
public class DiceGameConfig : BaseConfig
{
	public int max_turn_count;
	public int min_bet_size;
	public bool enemy_combo_upgrades_enabled = true;
	public string enemy_ai_mode = "heuristic";
	public string enemy_ai_scenario_id = string.Empty;
	public string modifiers_mode = "inventory";
	public string modifiers_set_id = string.Empty;
}

[Serializable]
public class DiceGameTacticsPoolConfig : BaseConfig
{
	public List<DiceGameTacticProfileConfig> tactics = new();

	public override void ParseConfig()
	{
		tactics ??= new List<DiceGameTacticProfileConfig>();
		for (int i = 0; i < tactics.Count; i++)
		{
			tactics[i] ??= new DiceGameTacticProfileConfig();
			tactics[i].ParseConfig();
		}
	}

	public bool TryValidateStatic(out string error)
	{
		if (tactics == null || tactics.Count == 0)
		{
			error = "[DiceGameTacticsPool] At least one tactic profile is required.";
			return false;
		}

		var uniqueIds = new HashSet<string>(StringComparer.Ordinal);
		for (int i = 0; i < tactics.Count; i++)
		{
			var profile = tactics[i];
			if (profile == null)
			{
				error = $"[DiceGameTacticsPool] Tactic profile at index {i} is null.";
				return false;
			}

			if (string.IsNullOrWhiteSpace(profile.id))
			{
				error = $"[DiceGameTacticsPool] Tactic profile at index {i} has empty id.";
				return false;
			}

			if (!uniqueIds.Add(profile.id))
			{
				error = $"[DiceGameTacticsPool] Duplicate tactic id '{profile.id}'.";
				return false;
			}

			if (profile.weight <= 0)
			{
				error = $"[DiceGameTacticsPool] Tactic '{profile.id}' has invalid weight '{profile.weight}'.";
				return false;
			}

			if (string.IsNullOrWhiteSpace(profile.enemy_ai_scenarios_path))
			{
				error = $"[DiceGameTacticsPool] Tactic '{profile.id}' has empty enemy_ai_scenarios_path.";
				return false;
			}

			if (string.IsNullOrWhiteSpace(profile.enemy_ai_scenario_schedule_path))
			{
				error = $"[DiceGameTacticsPool] Tactic '{profile.id}' has empty enemy_ai_scenario_schedule_path.";
				return false;
			}

			if (string.IsNullOrWhiteSpace(profile.modifiers_schedule_path))
			{
				error = $"[DiceGameTacticsPool] Tactic '{profile.id}' has empty modifiers_schedule_path.";
				return false;
			}
		}

		error = null;
		return true;
	}
}

[Serializable]
public class DiceGameTacticProfileConfig
{
	public string id = string.Empty;
	public int weight = 1;
	public string enemy_ai_scenarios_path = string.Empty;
	public string enemy_ai_scenario_schedule_path = string.Empty;
	public string modifiers_schedule_path = string.Empty;

	public void ParseConfig()
	{
		id = id?.Trim() ?? string.Empty;
		enemy_ai_scenarios_path = enemy_ai_scenarios_path?.Trim() ?? string.Empty;
		enemy_ai_scenario_schedule_path = enemy_ai_scenario_schedule_path?.Trim() ?? string.Empty;
		modifiers_schedule_path = modifiers_schedule_path?.Trim() ?? string.Empty;
	}
}

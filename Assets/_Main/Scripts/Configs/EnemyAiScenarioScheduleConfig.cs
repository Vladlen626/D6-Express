using System;
using System.Collections.Generic;

[Serializable]
public class EnemyAiScenarioScheduleConfig : BaseConfig
{
	public string default_scenario_id = string.Empty;
	public List<EnemyAiScenarioScheduleRuleConfig> rules = new();

	public override void ParseConfig()
	{
		rules ??= new List<EnemyAiScenarioScheduleRuleConfig>();
		for (int i = 0; i < rules.Count; i++)
		{
			rules[i] ??= new EnemyAiScenarioScheduleRuleConfig();
		}
	}

	public bool TryValidateStatic(out string error)
	{
		if (string.IsNullOrWhiteSpace(default_scenario_id))
		{
			error = "[EnemyAISchedule] default_scenario_id is required.";
			return false;
		}

		for (int i = 0; i < rules.Count; i++)
		{
			var rule = rules[i];
			if (string.IsNullOrWhiteSpace(rule.scenario_id))
			{
				error = $"[EnemyAISchedule] Rule #{i + 1}: scenario_id is required.";
				return false;
			}

			if (!rule.TryValidateStatic(out error))
			{
				error = $"[EnemyAISchedule] Rule #{i + 1}: {error}";
				return false;
			}
		}

		error = null;
		return true;
	}

	public bool TryResolveScenarioId(int level, int day, int match, out string scenarioId)
	{
		for (int i = 0; i < rules.Count; i++)
		{
			var rule = rules[i];
			if (rule.Matches(level, day, match))
			{
				scenarioId = rule.scenario_id;
				return true;
			}
		}

		scenarioId = default_scenario_id;
		return !string.IsNullOrWhiteSpace(scenarioId);
	}
}

[Serializable]
public class EnemyAiScenarioScheduleRuleConfig
{
	public string scenario_id = string.Empty;

	public int? level;
	public int? level_min;
	public int? level_max;

	public int? day;
	public int? day_min;
	public int? day_max;

	public int? match;
	public int? match_min;
	public int? match_max;

	public bool TryValidateStatic(out string error)
	{
		if (!ValidateAxis(level, level_min, level_max, "level", out error))
		{
			return false;
		}

		if (!ValidateAxis(day, day_min, day_max, "day", out error))
		{
			return false;
		}

		if (!ValidateAxis(match, match_min, match_max, "match", out error))
		{
			return false;
		}

		error = null;
		return true;
	}

	public bool Matches(int levelValue, int dayValue, int matchValue)
	{
		return MatchesAxis(level, level_min, level_max, levelValue)
		       && MatchesAxis(day, day_min, day_max, dayValue)
		       && MatchesAxis(match, match_min, match_max, matchValue);
	}

	private static bool ValidateAxis(int? exact, int? min, int? max, string axisName, out string error)
	{
		if (exact.HasValue && (min.HasValue || max.HasValue))
		{
			error = $"'{axisName}' exact value cannot be combined with '{axisName}_min'/'{axisName}_max'.";
			return false;
		}

		if (exact.HasValue && exact.Value <= 0)
		{
			error = $"'{axisName}' must be > 0.";
			return false;
		}

		if (min.HasValue && min.Value <= 0)
		{
			error = $"'{axisName}_min' must be > 0.";
			return false;
		}

		if (max.HasValue && max.Value <= 0)
		{
			error = $"'{axisName}_max' must be > 0.";
			return false;
		}

		if (min.HasValue && max.HasValue && min.Value > max.Value)
		{
			error = $"'{axisName}_min' cannot be greater than '{axisName}_max'.";
			return false;
		}

		error = null;
		return true;
	}

	private static bool MatchesAxis(int? exact, int? min, int? max, int value)
	{
		if (exact.HasValue)
		{
			return value == exact.Value;
		}

		if (min.HasValue && value < min.Value)
		{
			return false;
		}

		if (max.HasValue && value > max.Value)
		{
			return false;
		}

		return true;
	}
}

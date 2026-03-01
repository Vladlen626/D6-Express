using System;
using System.Collections.Generic;

[Serializable]
public class EnemyAiScenarioScheduleConfig : BaseConfig
{
	public const string WildcardKey = "*";

	public string default_scenario_id = string.Empty;
	public Dictionary<string, EnemyAiLevelScheduleNode> by_level = new();

	public override void ParseConfig()
	{
		by_level ??= new Dictionary<string, EnemyAiLevelScheduleNode>();
		var keys = new List<string>(by_level.Keys);
		for (int i = 0; i < keys.Count; i++)
		{
			var key = keys[i];
			if (!by_level.TryGetValue(key, out var node) || node == null)
			{
				node = new EnemyAiLevelScheduleNode();
				by_level[key] = node;
			}

			node.ParseConfig();
		}
	}

	public bool TryValidateStatic(out string error)
	{
		if (string.IsNullOrWhiteSpace(default_scenario_id))
		{
			error = "[EnemyAISchedule] default_scenario_id is required.";
			return false;
		}

		foreach (var pair in by_level)
		{
			if (!IsValidAxisKey(pair.Key))
			{
				error = $"[EnemyAISchedule] Invalid level key '{pair.Key}'. Use positive integer or '*'.";
				return false;
			}

			var node = pair.Value;
			if (node == null)
			{
				error = $"[EnemyAISchedule] Level node '{pair.Key}' is null.";
				return false;
			}

			if (!node.TryValidateStatic(pair.Key, out error))
			{
				return false;
			}
		}

		error = null;
		return true;
	}

	public bool TryResolveScenarioId(int level, int day, int match, out string scenarioId)
	{
		scenarioId = null;

		var orderedLevelKeys = new[] { level.ToString(), WildcardKey };
		for (int i = 0; i < orderedLevelKeys.Length; i++)
		{
			var levelKey = orderedLevelKeys[i];
			if (!by_level.TryGetValue(levelKey, out var levelNode) || levelNode == null)
			{
				continue;
			}

			if (levelNode.TryResolve(day, match, out scenarioId))
			{
				return true;
			}
		}

		scenarioId = default_scenario_id;
		return !string.IsNullOrWhiteSpace(scenarioId);
	}

	private static bool IsValidAxisKey(string key)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return false;
		}

		if (string.Equals(key, WildcardKey, StringComparison.Ordinal))
		{
			return true;
		}

		return int.TryParse(key, out var value) && value > 0;
	}
}

[Serializable]
public class EnemyAiLevelScheduleNode
{
	public string default_scenario_id = string.Empty;
	public Dictionary<string, EnemyAiDayScheduleNode> by_day = new();

	public void ParseConfig()
	{
		by_day ??= new Dictionary<string, EnemyAiDayScheduleNode>();
		var keys = new List<string>(by_day.Keys);
		for (int i = 0; i < keys.Count; i++)
		{
			var key = keys[i];
			if (!by_day.TryGetValue(key, out var node) || node == null)
			{
				node = new EnemyAiDayScheduleNode();
				by_day[key] = node;
			}

			node.ParseConfig();
		}
	}

	public bool TryValidateStatic(string levelKey, out string error)
	{
		foreach (var pair in by_day)
		{
			if (!IsValidAxisKey(pair.Key))
			{
				error = $"[EnemyAISchedule] Invalid day key '{pair.Key}' in level '{levelKey}'. Use positive integer or '*'.";
				return false;
			}

			if (pair.Value == null)
			{
				error = $"[EnemyAISchedule] Day node '{pair.Key}' in level '{levelKey}' is null.";
				return false;
			}

			if (!pair.Value.TryValidateStatic(levelKey, pair.Key, out error))
			{
				return false;
			}
		}

		if (!string.IsNullOrWhiteSpace(default_scenario_id))
		{
			error = null;
			return true;
		}

		if (by_day.Count == 0)
		{
			error = $"[EnemyAISchedule] Level '{levelKey}' has no default_scenario_id and no day rules.";
			return false;
		}

		error = null;
		return true;
	}

	public bool TryResolve(int day, int match, out string scenarioId)
	{
		scenarioId = null;
		var orderedDayKeys = new[] { day.ToString(), EnemyAiScenarioScheduleConfig.WildcardKey };
		for (int i = 0; i < orderedDayKeys.Length; i++)
		{
			var dayKey = orderedDayKeys[i];
			if (!by_day.TryGetValue(dayKey, out var dayNode) || dayNode == null)
			{
				continue;
			}

			if (dayNode.TryResolve(match, out scenarioId))
			{
				return true;
			}
		}

		scenarioId = default_scenario_id;
		return !string.IsNullOrWhiteSpace(scenarioId);
	}

	private static bool IsValidAxisKey(string key)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return false;
		}

		if (string.Equals(key, EnemyAiScenarioScheduleConfig.WildcardKey, StringComparison.Ordinal))
		{
			return true;
		}

		return int.TryParse(key, out var value) && value > 0;
	}
}

[Serializable]
public class EnemyAiDayScheduleNode
{
	public string default_scenario_id = string.Empty;
	public Dictionary<string, string> by_match = new();

	public void ParseConfig()
	{
		by_match ??= new Dictionary<string, string>();
	}

	public bool TryValidateStatic(string levelKey, string dayKey, out string error)
	{
		foreach (var pair in by_match)
		{
			if (!IsValidAxisKey(pair.Key))
			{
				error = $"[EnemyAISchedule] Invalid match key '{pair.Key}' in level '{levelKey}', day '{dayKey}'. Use positive integer or '*'.";
				return false;
			}

			if (string.IsNullOrWhiteSpace(pair.Value))
			{
				error = $"[EnemyAISchedule] Empty scenario id for match key '{pair.Key}' in level '{levelKey}', day '{dayKey}'.";
				return false;
			}
		}

		if (!string.IsNullOrWhiteSpace(default_scenario_id))
		{
			error = null;
			return true;
		}

		if (by_match.Count == 0)
		{
			error = $"[EnemyAISchedule] Day node level '{levelKey}', day '{dayKey}' has no default_scenario_id and no match rules.";
			return false;
		}

		error = null;
		return true;
	}

	public bool TryResolve(int match, out string scenarioId)
	{
		scenarioId = null;

		if (by_match.TryGetValue(match.ToString(), out var exact) && !string.IsNullOrWhiteSpace(exact))
		{
			scenarioId = exact;
			return true;
		}

		if (by_match.TryGetValue(EnemyAiScenarioScheduleConfig.WildcardKey, out var wildcard) && !string.IsNullOrWhiteSpace(wildcard))
		{
			scenarioId = wildcard;
			return true;
		}

		scenarioId = default_scenario_id;
		return !string.IsNullOrWhiteSpace(scenarioId);
	}

	private static bool IsValidAxisKey(string key)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return false;
		}

		if (string.Equals(key, EnemyAiScenarioScheduleConfig.WildcardKey, StringComparison.Ordinal))
		{
			return true;
		}

		return int.TryParse(key, out var value) && value > 0;
	}
}

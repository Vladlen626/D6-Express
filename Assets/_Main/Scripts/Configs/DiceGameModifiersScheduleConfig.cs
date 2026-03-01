using System;
using System.Collections.Generic;

[Serializable]
public class DiceGameModifiersScheduleConfig : BaseConfig
{
	public const string WildcardKey = "*";

	public string default_set_id = string.Empty;
	public Dictionary<string, DiceGameModifiersLevelNode> by_level = new();
	public Dictionary<string, DiceGameModifierSet> sets = new();

	public override void ParseConfig()
	{
		by_level ??= new Dictionary<string, DiceGameModifiersLevelNode>();
		sets ??= new Dictionary<string, DiceGameModifierSet>();

		var levelKeys = new List<string>(by_level.Keys);
		for (int i = 0; i < levelKeys.Count; i++)
		{
			var key = levelKeys[i];
			if (!by_level.TryGetValue(key, out var node) || node == null)
			{
				node = new DiceGameModifiersLevelNode();
				by_level[key] = node;
			}

			node.ParseConfig();
		}

		var setKeys = new List<string>(sets.Keys);
		for (int i = 0; i < setKeys.Count; i++)
		{
			var key = setKeys[i];
			if (!sets.TryGetValue(key, out var set) || set == null)
			{
				set = new DiceGameModifierSet();
				sets[key] = set;
			}

			set.ParseConfig();
		}
	}

	public bool TryValidateStatic(out string error)
	{
		if (string.IsNullOrWhiteSpace(default_set_id))
		{
			error = "[DiceGameModifiersSchedule] default_set_id is required.";
			return false;
		}

		if (!sets.ContainsKey(default_set_id))
		{
			error = $"[DiceGameModifiersSchedule] default_set_id '{default_set_id}' is missing in sets.";
			return false;
		}

		foreach (var pair in by_level)
		{
			if (!IsValidAxisKey(pair.Key))
			{
				error = $"[DiceGameModifiersSchedule] Invalid level key '{pair.Key}'. Use positive integer or '*'.";
				return false;
			}

			if (pair.Value == null)
			{
				error = $"[DiceGameModifiersSchedule] Level node '{pair.Key}' is null.";
				return false;
			}

			if (!pair.Value.TryValidateStatic(pair.Key, sets, out error))
			{
				return false;
			}
		}

		error = null;
		return true;
	}

	public bool TryResolveSet(int level, int day, int match, out DiceGameModifierSet set)
	{
		set = null;

		var orderedLevelKeys = new[] { level.ToString(), WildcardKey };
		for (int i = 0; i < orderedLevelKeys.Length; i++)
		{
			if (!by_level.TryGetValue(orderedLevelKeys[i], out var levelNode) || levelNode == null)
			{
				continue;
			}

			if (levelNode.TryResolve(day, match, sets, out set))
			{
				return true;
			}
		}

		return sets.TryGetValue(default_set_id, out set) && set != null;
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
public class DiceGameModifiersLevelNode
{
	public string default_set_id = string.Empty;
	public Dictionary<string, DiceGameModifiersDayNode> by_day = new();

	public void ParseConfig()
	{
		by_day ??= new Dictionary<string, DiceGameModifiersDayNode>();
		var keys = new List<string>(by_day.Keys);
		for (int i = 0; i < keys.Count; i++)
		{
			var key = keys[i];
			if (!by_day.TryGetValue(key, out var node) || node == null)
			{
				node = new DiceGameModifiersDayNode();
				by_day[key] = node;
			}

			node.ParseConfig();
		}
	}

	public bool TryValidateStatic(string levelKey, IReadOnlyDictionary<string, DiceGameModifierSet> sets, out string error)
	{
		if (!string.IsNullOrWhiteSpace(default_set_id) && !sets.ContainsKey(default_set_id))
		{
			error = $"[DiceGameModifiersSchedule] Level '{levelKey}' default_set_id '{default_set_id}' is missing in sets.";
			return false;
		}

		foreach (var pair in by_day)
		{
			if (!IsValidAxisKey(pair.Key))
			{
				error = $"[DiceGameModifiersSchedule] Invalid day key '{pair.Key}' in level '{levelKey}'. Use positive integer or '*'.";
				return false;
			}

			if (pair.Value == null)
			{
				error = $"[DiceGameModifiersSchedule] Day node '{pair.Key}' in level '{levelKey}' is null.";
				return false;
			}

			if (!pair.Value.TryValidateStatic(levelKey, pair.Key, sets, out error))
			{
				return false;
			}
		}

		error = null;
		return true;
	}

	public bool TryResolve(int day, int match, IReadOnlyDictionary<string, DiceGameModifierSet> sets, out DiceGameModifierSet set)
	{
		set = null;
		var orderedDayKeys = new[] { day.ToString(), DiceGameModifiersScheduleConfig.WildcardKey };
		for (int i = 0; i < orderedDayKeys.Length; i++)
		{
			if (!by_day.TryGetValue(orderedDayKeys[i], out var dayNode) || dayNode == null)
			{
				continue;
			}

			if (dayNode.TryResolve(match, sets, out set))
			{
				return true;
			}
		}

		return !string.IsNullOrWhiteSpace(default_set_id)
		       && sets.TryGetValue(default_set_id, out set)
		       && set != null;
	}

	private static bool IsValidAxisKey(string key)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return false;
		}

		if (string.Equals(key, DiceGameModifiersScheduleConfig.WildcardKey, StringComparison.Ordinal))
		{
			return true;
		}

		return int.TryParse(key, out var value) && value > 0;
	}
}

[Serializable]
public class DiceGameModifiersDayNode
{
	public string default_set_id = string.Empty;
	public Dictionary<string, string> by_match = new();

	public void ParseConfig()
	{
		by_match ??= new Dictionary<string, string>();
	}

	public bool TryValidateStatic(
		string levelKey,
		string dayKey,
		IReadOnlyDictionary<string, DiceGameModifierSet> sets,
		out string error)
	{
		if (!string.IsNullOrWhiteSpace(default_set_id) && !sets.ContainsKey(default_set_id))
		{
			error = $"[DiceGameModifiersSchedule] Level '{levelKey}', day '{dayKey}' default_set_id '{default_set_id}' is missing in sets.";
			return false;
		}

		foreach (var pair in by_match)
		{
			if (!IsValidAxisKey(pair.Key))
			{
				error = $"[DiceGameModifiersSchedule] Invalid match key '{pair.Key}' in level '{levelKey}', day '{dayKey}'. Use positive integer or '*'.";
				return false;
			}

			if (string.IsNullOrWhiteSpace(pair.Value))
			{
				error = $"[DiceGameModifiersSchedule] Empty set id for match key '{pair.Key}' in level '{levelKey}', day '{dayKey}'.";
				return false;
			}

			if (!sets.ContainsKey(pair.Value))
			{
				error = $"[DiceGameModifiersSchedule] Set id '{pair.Value}' for match key '{pair.Key}' in level '{levelKey}', day '{dayKey}' is missing in sets.";
				return false;
			}
		}

		error = null;
		return true;
	}

	public bool TryResolve(int match, IReadOnlyDictionary<string, DiceGameModifierSet> sets, out DiceGameModifierSet set)
	{
		set = null;

		if (by_match.TryGetValue(match.ToString(), out var exactSetId)
		    && !string.IsNullOrWhiteSpace(exactSetId)
		    && sets.TryGetValue(exactSetId, out set)
		    && set != null)
		{
			return true;
		}

		if (by_match.TryGetValue(DiceGameModifiersScheduleConfig.WildcardKey, out var wildcardSetId)
		    && !string.IsNullOrWhiteSpace(wildcardSetId)
		    && sets.TryGetValue(wildcardSetId, out set)
		    && set != null)
		{
			return true;
		}

		return !string.IsNullOrWhiteSpace(default_set_id)
		       && sets.TryGetValue(default_set_id, out set)
		       && set != null;
	}

	private static bool IsValidAxisKey(string key)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return false;
		}

		if (string.Equals(key, DiceGameModifiersScheduleConfig.WildcardKey, StringComparison.Ordinal))
		{
			return true;
		}

		return int.TryParse(key, out var value) && value > 0;
	}
}

[Serializable]
public class DiceGameModifierSet
{
	public string[] player_modifiers = Array.Empty<string>();
	public string[] enemy_modifiers = Array.Empty<string>();

	public void ParseConfig()
	{
		player_modifiers ??= Array.Empty<string>();
		enemy_modifiers ??= Array.Empty<string>();
	}
}

public static class DiceGameModifiersMode
{
	public const string Inventory = "inventory";
	public const string Scripted = "scripted";
}

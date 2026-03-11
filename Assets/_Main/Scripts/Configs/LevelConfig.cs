using System;
using System.Collections.Generic;

[Serializable]
public class LevelConfig : BaseConfig
{
	public int days;
	public string station_id;
	public int ticks_per_day;
	public int cash_goal;
	public TargetScoreScheduleConfig target_score_schedule = new();

	public override void ParseConfig()
	{
		target_score_schedule ??= new TargetScoreScheduleConfig();
		target_score_schedule.ParseConfig();
	}

	public bool TryResolveTargetScore(int day, int match, out int targetScore)
	{
		targetScore = 0;
		if (target_score_schedule == null)
		{
			return false;
		}

		return target_score_schedule.TryResolve(day, match, out targetScore);
	}
}

[Serializable]
public class TargetScoreScheduleConfig
{
	private const string WildcardKey = "*";
	public Dictionary<string, TargetScoreDayNodeConfig> by_day = new();

	public void ParseConfig()
	{
		by_day ??= new Dictionary<string, TargetScoreDayNodeConfig>();
		var keys = new List<string>(by_day.Keys);
		for (int i = 0; i < keys.Count; i++)
		{
			var key = keys[i];
			if (!by_day.TryGetValue(key, out var node) || node == null)
			{
				node = new TargetScoreDayNodeConfig();
				by_day[key] = node;
			}

			node.ParseConfig();
		}
	}

	public bool TryResolve(int day, int match, out int targetScore)
	{
		targetScore = 0;
		var orderedDayKeys = new[] { day.ToString(), WildcardKey };
		for (int i = 0; i < orderedDayKeys.Length; i++)
		{
			if (!by_day.TryGetValue(orderedDayKeys[i], out var dayNode) || dayNode == null)
			{
				continue;
			}

			if (dayNode.TryResolve(match, out targetScore))
			{
				return targetScore > 0;
			}
		}

		return false;
	}
}

[Serializable]
public class TargetScoreDayNodeConfig
{
	private const string WildcardKey = "*";
	public Dictionary<string, int> by_match = new();

	public void ParseConfig()
	{
		by_match ??= new Dictionary<string, int>();
	}

	public bool TryResolve(int match, out int targetScore)
	{
		targetScore = 0;
		if (by_match.TryGetValue(match.ToString(), out var exact))
		{
			targetScore = exact;
			return true;
		}

		if (by_match.TryGetValue(WildcardKey, out var wildcard))
		{
			targetScore = wildcard;
			return true;
		}

		return false;
	}
}

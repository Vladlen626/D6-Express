using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class EnemyAiScenarioConfig : BaseConfig
{
	public string mode = "scripted";
	public bool strict_validation = true;
	public string on_mismatch = EnemyAiMismatchMode.FailScenario;
	public EnemyAiSetupConfig enemy_setup = new();
	public List<EnemyAiTurnConfig> turns = new();
	public EnemyAiExpectedResultConfig expected_result = new();

	public override void ParseConfig()
	{
		enemy_setup ??= new EnemyAiSetupConfig();
		enemy_setup.ParseConfig();

		turns ??= new List<EnemyAiTurnConfig>();
		for (int i = 0; i < turns.Count; i++)
		{
			turns[i] ??= new EnemyAiTurnConfig();
			turns[i].ParseConfig();
		}

		expected_result ??= new EnemyAiExpectedResultConfig();
		expected_result.ParseConfig();

		if (string.IsNullOrWhiteSpace(on_mismatch))
		{
			on_mismatch = EnemyAiMismatchMode.FailScenario;
		}
	}

	public bool TryValidateStatic(out string error)
	{
		if (!string.Equals(mode, EnemyAiMode.Scripted, StringComparison.OrdinalIgnoreCase))
		{
			error = $"Scenario '{id}': unsupported mode '{mode}'.";
			return false;
		}

		if (!string.Equals(on_mismatch, EnemyAiMismatchMode.FailScenario, StringComparison.OrdinalIgnoreCase))
		{
			error = $"Scenario '{id}': unsupported on_mismatch '{on_mismatch}'.";
			return false;
		}

		if (!strict_validation)
		{
			error = $"Scenario '{id}': strict_validation must be true for scripted mode.";
			return false;
		}

		if (enemy_setup.use_modifiers)
		{
			error = $"Scenario '{id}': enemy_setup.use_modifiers is deprecated. Configure modifiers via dice_game_rules.modifiers_mode and dice_game_modifiers_schedule.json.";
			return false;
		}

		if (enemy_setup.dice_in_hand == null || enemy_setup.dice_in_hand.Length == 0)
		{
			error = $"Scenario '{id}': enemy_setup.dice_in_hand must contain at least one die id.";
			return false;
		}

		if (turns == null || turns.Count == 0)
		{
			error = $"Scenario '{id}': turns must contain at least one turn.";
			return false;
		}

		for (int i = 0; i < turns.Count; i++)
		{
			var turn = turns[i];
			if (turn.steps == null || turn.steps.Count == 0)
			{
				error = $"Scenario '{id}': turn #{i + 1} has no steps.";
				return false;
			}

			if (turn.steps[turn.steps.Count - 1].action_type != EnemyAiStepAction.Pass)
			{
				error = $"Scenario '{id}': turn #{i + 1} must end with a pass step.";
				return false;
			}

			int passCount = 0;
			for (int j = 0; j < turn.steps.Count; j++)
			{
				var step = turn.steps[j];
				if (step.action_type == EnemyAiStepAction.Unknown)
				{
					error = $"Scenario '{id}': turn #{i + 1}, step #{j + 1} has unknown action '{step.action}'.";
					return false;
				}

				if (step.action_type == EnemyAiStepAction.Pass)
				{
					passCount++;
					continue;
				}

				if (step.forced_values == null || step.forced_values.Length == 0)
				{
					error = $"Scenario '{id}': turn #{i + 1}, step #{j + 1} roll has empty forced_values.";
					return false;
				}

				if (step.save_unbanked_indexes == null || step.save_unbanked_indexes.Length == 0)
				{
					error = $"Scenario '{id}': turn #{i + 1}, step #{j + 1} roll has empty save_unbanked_indexes.";
					return false;
				}

				if (step.save_unbanked_indexes.Distinct().Count() != step.save_unbanked_indexes.Length)
				{
					error = $"Scenario '{id}': turn #{i + 1}, step #{j + 1} has duplicate save_unbanked_indexes.";
					return false;
				}

				if (step.expected_saved_score.HasValue && step.expected_saved_score.Value < 0)
				{
					error = $"Scenario '{id}': turn #{i + 1}, step #{j + 1} has negative expected_saved_score.";
					return false;
				}
			}

			if (passCount != 1)
			{
				error = $"Scenario '{id}': turn #{i + 1} must contain exactly one pass step.";
				return false;
			}
		}

		if (expected_result.completed_in_enemy_turns <= 0)
		{
			error = $"Scenario '{id}': expected_result.completed_in_enemy_turns must be > 0.";
			return false;
		}

		if (expected_result.completed_in_enemy_turns != turns.Count)
		{
			error = $"Scenario '{id}': expected_result.completed_in_enemy_turns must be equal to turns count.";
			return false;
		}

		if (expected_result.enemy_final_banked_score < 0)
		{
			error = $"Scenario '{id}': expected_result.enemy_final_banked_score must be >= 0.";
			return false;
		}

		error = null;
		return true;
	}
}

[Serializable]
public class EnemyAiSetupConfig
{
	public bool use_modifiers = false;
	public string[] dice_in_hand = Array.Empty<string>();

	public void ParseConfig()
	{
		dice_in_hand ??= Array.Empty<string>();
	}
}

[Serializable]
public class EnemyAiTurnConfig
{
	public int enemy_turn;
	public List<EnemyAiStepConfig> steps = new();

	public void ParseConfig()
	{
		steps ??= new List<EnemyAiStepConfig>();
		for (int i = 0; i < steps.Count; i++)
		{
			steps[i] ??= new EnemyAiStepConfig();
			steps[i].ParseConfig();
		}
	}
}

[Serializable]
public class EnemyAiStepConfig
{
	public string action;
	public int[] forced_values;
	public int[] save_unbanked_indexes;
	public int? expected_saved_score;

	[NonSerialized]
	public EnemyAiStepAction action_type;

	public void ParseConfig()
	{
		action_type = ParseAction(action);
	}

	private static EnemyAiStepAction ParseAction(string rawAction)
	{
		if (string.Equals(rawAction, EnemyAiAction.Roll, StringComparison.OrdinalIgnoreCase))
		{
			return EnemyAiStepAction.Roll;
		}

		if (string.Equals(rawAction, EnemyAiAction.Pass, StringComparison.OrdinalIgnoreCase))
		{
			return EnemyAiStepAction.Pass;
		}

		return EnemyAiStepAction.Unknown;
	}
}

[Serializable]
public class EnemyAiExpectedResultConfig
{
	public int completed_in_enemy_turns;
	public int enemy_final_banked_score;
	public bool enemy_reaches_target;

	public void ParseConfig()
	{
	}
}

public enum EnemyAiStepAction
{
	Unknown = 0,
	Roll = 1,
	Pass = 2
}

public static class EnemyAiMode
{
	public const string Heuristic = "heuristic";
	public const string Scripted = "scripted";
}

public static class EnemyAiAction
{
	public const string Roll = "roll";
	public const string Pass = "pass";
}

public static class EnemyAiMismatchMode
{
	public const string FailScenario = "fail_scenario";
}

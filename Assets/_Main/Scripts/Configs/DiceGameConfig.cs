using System;

[Serializable]
public class DiceGameConfig : BaseConfig
{
	public int target_score;
	public int max_turn_count;
	public int min_bet_size;
	public bool enemy_combo_upgrades_enabled = true;
	public string enemy_ai_mode = "heuristic";
	public string enemy_ai_scenario_id = string.Empty;
	public string modifiers_mode = "inventory";
	public string modifiers_set_id = string.Empty;
}

using System;

[Serializable]
public class DiceGameConfig : BaseConfig
{
	public int target_score;
	public int max_turn_count;
	public int min_bet_size;
	public bool enemy_combo_upgrades_enabled = true;
}

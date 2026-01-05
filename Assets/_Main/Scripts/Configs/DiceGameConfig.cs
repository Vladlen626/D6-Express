using System;

[Serializable]
public class DiceGameConfig : BaseConfig
{
	public int target_score;
	public int max_turn_count;
	public int min_bet_size;
}
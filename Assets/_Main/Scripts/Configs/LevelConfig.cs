using System;

[Serializable]
public class LevelConfig : BaseConfig
{
	public int days;
	public int ticks_per_day;
	public int cash_goal;
}
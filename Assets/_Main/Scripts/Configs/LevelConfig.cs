using System;

[Serializable]
public class LevelConfig : BaseConfig
{
	public int days;
	public string next_station_id;
	public int ticks_per_day;
	public int cash_goal;
	public int ticket_price;
}
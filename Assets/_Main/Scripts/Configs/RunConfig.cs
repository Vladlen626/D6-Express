using System;

[Serializable]
public class RunConfig : BaseConfig
{
	public string first_station_id;
	public LevelConfig[] levels;
}
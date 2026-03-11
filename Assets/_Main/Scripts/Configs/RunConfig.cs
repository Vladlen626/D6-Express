using System;

[Serializable]
public class RunConfig : BaseConfig
{
	public LevelConfig[] levels;

	public override void ParseConfig()
	{
		levels ??= Array.Empty<LevelConfig>();
		for (int i = 0; i < levels.Length; i++)
		{
			if (levels[i] == null)
			{
				levels[i] = new LevelConfig();
			}

			levels[i].ParseConfig();
		}
	}
}

using System;

[Serializable]
public class RunConfig : BaseConfig
{
	public LevelConfig[] levels;
	public float shop_item_price_multiplier = 1f;

	public override void ParseConfig()
	{
		if (shop_item_price_multiplier <= 0f)
		{
			shop_item_price_multiplier = 1f;
		}

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

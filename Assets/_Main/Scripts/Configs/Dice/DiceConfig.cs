using System;

[Serializable]
public class DiceConfig : BaseConfig
{
	public string name;
	public int[] weights;
	public string description;
	public string rarity;
	public Rarity rarityEnum;
	public int price;

	public override void ParseConfig()
	{
		switch (rarity)
		{
			case "Common":
				rarityEnum = Rarity.COMMON;
				break;
			case "Uncommon":
				rarityEnum = Rarity.UNCOMMON;
				break;
			case "Rare":
				rarityEnum = Rarity.RARE;
				break;
			case "Legendary":
				rarityEnum = Rarity.LEGENDARY;
				break;
		}
	}
}

public enum Rarity
{
	COMMON,
	UNCOMMON,
	RARE,
	LEGENDARY
}
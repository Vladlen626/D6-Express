using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Serializable]
public class ItemCatalogEntry : BaseConfig
{
    public string type;
    public string nameKey;
    public string descriptionKey;
    public string rarity;
    public int price;
    public string visualId;
    public JObject data;

    [JsonIgnore]
    public ItemCatalogType typeEnum;

    [JsonIgnore]
    public Rarity rarityEnum;

    public override void ParseConfig()
    {
        if (!string.IsNullOrEmpty(type))
        {
            var normalized = type.Trim();
            if (string.Equals(normalized, "Dice", StringComparison.OrdinalIgnoreCase))
            {
                typeEnum = ItemCatalogType.Dice;
            }
            else if (string.Equals(normalized, "ModifierItem", StringComparison.OrdinalIgnoreCase))
            {
                typeEnum = ItemCatalogType.ModifierItem;
            }
            else
            {
                typeEnum = ItemCatalogType.Unknown;
            }
        }
        else
        {
            typeEnum = ItemCatalogType.Unknown;
        }

        if (string.IsNullOrEmpty(rarity))
        {
            rarityEnum = Rarity.COMMON;
            return;
        }

        switch (rarity.Trim())
        {
            case "Common":
            case "common":
                rarityEnum = Rarity.COMMON;
                break;
            case "Uncommon":
            case "uncommon":
                rarityEnum = Rarity.UNCOMMON;
                break;
            case "Rare":
            case "rare":
                rarityEnum = Rarity.RARE;
                break;
            case "Legendary":
            case "legendary":
                rarityEnum = Rarity.LEGENDARY;
                break;
            default:
                rarityEnum = Rarity.COMMON;
                break;
        }
    }

    public bool TryGetDiceData(out DiceItemData diceData)
    {
        diceData = null;
        if (data == null)
        {
            return false;
        }

        try
        {
            diceData = data.ToObject<DiceItemData>();
            return diceData != null;
        }
        catch
        {
            return false;
        }
    }
}

[Serializable]
public class DiceItemData
{
    public int[] weights;
}

public enum ItemCatalogType
{
    Unknown,
    Dice,
    ModifierItem
}

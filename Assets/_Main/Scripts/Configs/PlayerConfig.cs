using System;
using Newtonsoft.Json;

[Serializable]
public class PlayerConfig : BaseConfig
{
	public int cash;
	public string[] dices;
	public string[] modifierItems;
	public int modifierItemsCapacity = 6;

	[JsonProperty("modifiers")]
	private string[] legacyModifiers;

	public override void ParseConfig()
	{
		dices ??= Array.Empty<string>();

		if (modifierItemsCapacity < 0)
		{
			throw new InvalidOperationException(
				$"[PlayerConfig] modifierItemsCapacity cannot be negative (actual: {modifierItemsCapacity}).");
		}

		if (modifierItems == null)
		{
			modifierItems = legacyModifiers ?? Array.Empty<string>();
		}
	}
}

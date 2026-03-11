using System;
using Newtonsoft.Json;

[Serializable]
public class PlayerConfig : BaseConfig
{
	public int cash;
	public string[] dices;
	public string[] modifierItems;

	[JsonProperty("modifiers")]
	private string[] legacyModifiers;

	public override void ParseConfig()
	{
		dices ??= Array.Empty<string>();

		if (modifierItems == null)
		{
			modifierItems = legacyModifiers ?? Array.Empty<string>();
		}
	}
}

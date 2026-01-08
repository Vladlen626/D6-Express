using System;

[Serializable]
public class BaseConfig : IConfig
{
	public string id { get; set; }
	public virtual void ParseConfig()
	{
	}
}
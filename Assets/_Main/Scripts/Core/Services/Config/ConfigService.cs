using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using PlatformCore.Services;
using UnityEngine;

public class ConfigService : IService
{
	private readonly IResourceService _resourceService;
	private readonly ILoggerService _loggerService;

	// список для сохранения порядка JSON
	private readonly Dictionary<System.Type, List<IConfig>> _configsList = new();

	public ConfigService(IResourceService resourceService, ILoggerService loggerService)
	{
		_resourceService = resourceService;
		_loggerService = loggerService;
	}

	public async UniTask<Dictionary<string, T>> GetConfigsAsync<T>(string resourcePath) where T : IConfig
	{
		var type = typeof(T);

		var jsonTextAsset = await _resourceService.LoadAsync<TextAsset>(resourcePath);
		if (jsonTextAsset == null)
		{
			_loggerService.LogError($"[ConfigService] Failed to load config at {resourcePath}");
			return new Dictionary<string, T>();
		}

		List<T> list;

		try
		{
			list = JsonConvert.DeserializeObject<List<T>>(jsonTextAsset.text);
		}
		catch
		{
			var single = JsonConvert.DeserializeObject<T>(jsonTextAsset.text);
			list = new List<T> { single };
		}

		var dict = new Dictionary<string, T>();
		foreach (var item in list)
		{
			var key = string.IsNullOrEmpty(item.id) ? "default" : item.id;
			item.ParseConfig();
			dict[key] = item;
		}

		_configsList[type] = list.Cast<IConfig>().ToList();

		return dict;
	}


	public async UniTask<T> GetFirstOrDefaultAsync<T>(string resourcePath) where T : IConfig
	{
		var type = typeof(T);

		if (!_configsList.ContainsKey(type))
			await GetConfigsAsync<T>(resourcePath);

		if (_configsList[type].Count == 0)
			return default;

		return (T)_configsList[type][0];
	}

	public void Dispose()
	{
	}
}
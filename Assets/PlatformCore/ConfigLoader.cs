using UnityEngine;
using System.Collections.Generic;

public class ConfigLoader<T> where T : class
{
	private static Dictionary<string, T> _cache = new Dictionary<string, T>();

	public static T Load(string path)
	{
		if (_cache.TryGetValue(path, out var cached))
		{
			return cached;
		}

		var file = Resources.Load<TextAsset>(path);
		if (file == null)
		{
			Debug.LogError($"Config not found: {path}");
			return null;
		}

		var data = JsonUtility.FromJson<T>(file.text);
		_cache[path] = data;
		return data;
	}

	public static void ClearCache() => _cache.Clear();
}
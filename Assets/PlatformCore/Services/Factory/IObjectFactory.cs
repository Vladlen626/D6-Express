using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlatformCore.Services.Factory
{
	public interface IObjectFactory
	{
		UniTask<GameObject> CreateAsync(
			string address,
			Vector3 position,
			Quaternion rotation,
			Transform parent = null);

		UniTask<T> CreateAsync<T>(
			string address,
			Vector3 position,
			Quaternion rotation,
			Transform parent = null) where T : Component;

		void Destroy(GameObject obj);
	}
}
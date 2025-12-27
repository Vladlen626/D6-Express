using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlatformCore.Services.Pool
{
	public interface IPoolService
	{
		UniTask CreatePoolAsync<T>(string prefabPath, int initialSize = 10, Transform parent = null)
			where T : Component;

		T Rent<T>(Vector3 position = default, Quaternion rotation = default, Transform parent = null)
			where T : Component;

		void Return<T>(T component) where T : Component;
		
		void ReturnDelayed<T>(T component, float delay) where T : Component;
		
		(int active, int inactive) GetPoolStats<T>() where T : Component;
		
		void ClearPool<T>() where T : Component;
	}
}
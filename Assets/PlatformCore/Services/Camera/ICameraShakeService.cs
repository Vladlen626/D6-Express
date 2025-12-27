using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlatformCore.Services
{
	public interface ICameraShakeService
	{
		UniTask ShakeAsync(float intensity, float duration);
		void StopShake();
		bool IsShaking { get; }
	}
}
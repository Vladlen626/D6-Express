using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace PlatformCore.Services
{
	public interface ICameraShakeService
	{
		UniTask ShakeAsync(float intensity, float duration);
		UniTask ShakeAsync(CinemachineCamera camera, float intensity, float duration);
		void StopShake();
		bool IsShaking { get; }
	}
}
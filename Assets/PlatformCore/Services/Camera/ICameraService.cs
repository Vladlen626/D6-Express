using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace PlatformCore.Services
{
	public interface ICameraService : ICameraShakeService
	{
		CameraStateEnum ActiveCameraState { get; }
		event Action<CameraStateEnum> ActiveCameraChanged;
		void SetActiveCamera(CameraStateEnum state);
		UniTask SetActiveCameraAsync(CameraStateEnum state, CancellationToken ct = default);
		void AddCamera(CameraStateEnum state, CinemachineCamera camera);
		void AttachPlayerCameraTo(Transform target);
		Transform GetCameraTransform();
		void SetFOV(float fov);
		float GetFOV();
		void SetDutch(float degrees);
		float GetDutch();
	}
}
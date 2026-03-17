using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlatformCore.Services.Factory;
using Unity.Cinemachine;
using UnityEngine;

namespace PlatformCore.Services
{
	public enum CameraStateEnum
	{
		FirstPerson,
		MainMenu,
		TrainWatch,
		DiceGame,
		DiceGameCombinations,
		Inventory
	}

	public class CameraService : BaseAsyncService, ICameraService
	{
		private const string PlayerCamera = "PlayerCamera";
		private readonly IObjectFactory _objectFactory;
		private readonly Transform _cameraParent;

		private CinemachineBasicMultiChannelPerlin _noise;
		private CinemachineBasicMultiChannelPerlin _activeShakeNoise;
		private CancellationTokenSource _shakeCts;
		public bool IsShaking { get; private set; }

		private CinemachineCamera currentCamera;
		private CinemachineBrain brain;
		public CameraStateEnum ActiveCameraState { get; private set; }
		public event Action<CameraStateEnum> ActiveCameraChanged;

		private readonly Dictionary<CameraStateEnum, CinemachineCamera> allCameras =
			new Dictionary<CameraStateEnum, CinemachineCamera>();

		public CameraService(IObjectFactory objectFactory, Transform cameraParent = null)
		{
			_objectFactory = objectFactory;
			_cameraParent = cameraParent;
		}

		protected override async UniTask OnPreInitializeAsync(CancellationToken ct)
		{
			brain = Camera.main.GetComponent<CinemachineBrain>();

			var _camera = await _objectFactory.CreateAsync<CinemachineCamera>(ResourcePaths.Player.CinemachineCamera,
				Vector3.zero, Quaternion.identity, _cameraParent);
			_noise = (CinemachineBasicMultiChannelPerlin)_camera.GetCinemachineComponent(CinemachineCore.Stage.Noise);
			_camera.name = PlayerCamera;
			allCameras.Add(CameraStateEnum.FirstPerson, _camera);

			SetActiveCamera(CameraStateEnum.FirstPerson);
		}

		public override void Dispose()
		{
			StopShake();

			foreach (var cam in allCameras.Values)
			{
				if (cam)
				{
					_objectFactory.Destroy(cam.gameObject);
				}
			}

			allCameras.Clear();
			currentCamera = null;
		}

		public void AttachPlayerCameraTo(Transform target)
		{
			var _camera = allCameras[CameraStateEnum.FirstPerson];
			if (_camera == null || target == null)
			{
				return;
			}

			_camera.transform.SetParent(target);
			_camera.transform.localPosition = Vector3.zero;
			_camera.transform.localRotation = Quaternion.identity;

			_camera.Follow = null;
			_camera.LookAt = null;
		}
		
		public async UniTask SetActiveCameraAsync(CameraStateEnum state, CancellationToken ct = default)
		{
			if (allCameras == null || !allCameras.ContainsKey(state))
			{
				Debug.LogWarning($"Camera {state} not found!");
				return;
			}
			
			if (brain == null)
			{
				Debug.LogWarning("No CinemachineBrain found on main camera!");
				SetActiveCamera(state); // fallback
				return;
			}
			
			SetActiveCamera(state);

			await UniTask.WaitUntil(() => !brain.IsBlending, cancellationToken: ct);
		}

		public void SetActiveCamera(CameraStateEnum state)
		{
			if (allCameras == null || !allCameras.ContainsKey(state))
			{
				Debug.LogWarning($"Camera {state} not found!");
				return;
			}

			if (currentCamera == allCameras[state] && ActiveCameraState == state)
			{
				return;
			}

			StopShake();

			if (currentCamera != null)
			{
				currentCamera.gameObject.SetActive(false);
			}

			currentCamera = allCameras[state];
			currentCamera.gameObject.SetActive(true);
			ActiveCameraState = state;
			ActiveCameraChanged?.Invoke(state);


			_noise = (CinemachineBasicMultiChannelPerlin)currentCamera.GetCinemachineComponent(CinemachineCore.Stage
				.Noise);
		}

		public void AddCamera(CameraStateEnum state, CinemachineCamera camera)
		{
			if (camera == null || allCameras.ContainsKey(state))
				return;

			camera.gameObject.SetActive(false); // деактивируем по умолчанию
			allCameras.Add(state, camera);
		}

		public Transform GetCameraTransform()
		{
			return currentCamera?.transform;
		}


		public void SetFOV(float fov)
		{
			if (currentCamera != null)
			{
				currentCamera.Lens.FieldOfView = fov;
			}
		}

		public float GetFOV()
		{
			return currentCamera != null ? currentCamera.Lens.FieldOfView : 60f;
		}

		public void SetDutch(float degrees)
		{
			if (currentCamera != null)
				currentCamera.Lens.Dutch = degrees;
		}

		public float GetDutch()
		{
			return currentCamera != null ? currentCamera.Lens.Dutch : 0f;
		}


		// ReSharper disable Unity.PerformanceAnalysis
		public async UniTask ShakeAsync(float intensity, float duration)
		{
			var noise = _noise;
			if (noise == null || IsShaking)
			{
				return;
			}

			IsShaking = true;
			_shakeCts?.Cancel();
			_shakeCts?.Dispose();
			_shakeCts = new CancellationTokenSource();
			var shakeCts = _shakeCts;
			_activeShakeNoise = noise;

			noise.AmplitudeGain = intensity;
			noise.FrequencyGain = intensity * 1.5f;

			try
			{
				await UniTask.WaitForSeconds(duration, cancellationToken: shakeCts.Token);
			}
			catch (OperationCanceledException)
			{
			}
			finally
			{
				ResetNoise(noise);

				if (ReferenceEquals(_shakeCts, shakeCts))
				{
					_shakeCts.Dispose();
					_shakeCts = null;
					_activeShakeNoise = null;
					IsShaking = false;
				}
			}
		}

		public void StopShake()
		{
			_shakeCts?.Cancel();
			_shakeCts?.Dispose();
			_shakeCts = null;

			ResetNoise(_activeShakeNoise);

			if (!ReferenceEquals(_activeShakeNoise, _noise))
			{
				ResetNoise(_noise);
			}

			_activeShakeNoise = null;
			IsShaking = false;
		}

		private static void ResetNoise(CinemachineBasicMultiChannelPerlin noise)
		{
			if (noise == null)
			{
				return;
			}

			noise.AmplitudeGain = 0;
			noise.FrequencyGain = 0;
		}
	}
}
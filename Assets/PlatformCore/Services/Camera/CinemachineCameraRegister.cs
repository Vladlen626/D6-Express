using PlatformCore.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace PlatformCore.Services
{
	// TODO: ВАААЩЕЕЕ НЕ НРАВИТСЯ, но на скорую руку так
	[RequireComponent(typeof(CinemachineCamera))]
	public class CinemachineCameraRegister : MonoBehaviour
	{
		[SerializeField]
		private CameraStateEnum cameraStateEnum;
		private void Awake()
		{
			var cinemachineCamera = GetComponent<CinemachineCamera>();
			Locator.Resolve<ICameraService>().AddCamera(cameraStateEnum, cinemachineCamera);
		}
	}
}
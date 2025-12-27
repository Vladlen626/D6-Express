using PlatformCore.Services;
using UnityEngine;

namespace PlatformCore.Services
{
	public interface ICameraService : ICameraShakeService
	{
		void AttachTo(Transform target);
		Transform GetCameraTransform();
		void SetFOV(float fov);

		float GetFOV();
		void SetDutch(float degrees);
		float GetDutch();
	}
}
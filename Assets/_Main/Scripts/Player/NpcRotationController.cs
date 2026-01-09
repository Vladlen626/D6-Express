using System;
using UnityEngine;

// todo: никакого монобеха
public class NpcRotationController : MonoBehaviour
{
	private CharacterView characterView;
	private PlayerStateModel playerStateModel;

	private CameraState cameraState;

	private Vector3? target;

	private Vector3? lastDirection;

	public Vector3? Target
	{
		get => target;
		set
		{
			target = value;
			TargetChanged?.Invoke(target);
		}
	}

	public event Action<Vector3?> TargetChanged;

	public void Initialize(CharacterView characterView, PlayerStateModel playerStateModel)
	{
		this.characterView = characterView;
		this.playerStateModel = playerStateModel;

		cameraState = characterView.GetCameraState(CharacterState.DEFAULT);
		playerStateModel.StatesChanged += OnCharacterStateChanged;
	}

	void Update()
	{
		if (!Target.HasValue && !lastDirection.HasValue) return;

		if (Target.HasValue && !lastDirection.HasValue)
		{
			lastDirection = characterView.transform.forward;
		}

		if (cameraState.rotationType == RotationType.HEAD)
		{
			var direction = Target.HasValue ? (Target.Value - characterView.Head.position).normalized : lastDirection.Value;
			RotateHeadTowardDir(direction);
		}
		else
		{
			var direction = Target.HasValue ? (Target.Value - characterView.transform.position).normalized : lastDirection.Value;
			RotateBodyTowardDir(direction);
			RotateHeadToFollowBody();
		}
	}

	void RotateBodyTowardDir(Vector3 dirWorld)
	{
		dirWorld.y = 0f;
		if (dirWorld.sqrMagnitude < 0.000001f) return;

		Quaternion targetRot = Quaternion.LookRotation(dirWorld.normalized, Vector3.up);
		float step = characterView.mouseSensitivity * Time.deltaTime;

		characterView.transform.rotation =
			Quaternion.RotateTowards(characterView.transform.rotation, targetRot, step);
	}

	void RotateHeadTowardDir(Vector3 dirWorld)
	{
		Vector3 dirLocal = characterView.transform.InverseTransformDirection(dirWorld);
		dirLocal.y = 0f;
		if (dirLocal.sqrMagnitude < 0.000001f) return;

		float desiredYaw = Mathf.Atan2(dirLocal.x, dirLocal.z) * Mathf.Rad2Deg;

		float step = characterView.mouseSensitivity * Time.deltaTime;
		float currentYaw = characterView.Head.localEulerAngles.y;
		float newYaw = Mathf.MoveTowardsAngle(currentYaw, desiredYaw, step);

		characterView.Head.localRotation = Quaternion.Euler(0f, newYaw, 0f);
	}

	void RotateHeadToFollowBody()
	{
		float step = characterView.mouseSensitivity * Time.deltaTime;

		float currentYaw = characterView.Head.localEulerAngles.y;
		float newYaw = Mathf.MoveTowardsAngle(currentYaw, 0f, step);

		characterView.Head.localRotation = Quaternion.Euler(0f, newYaw, 0f);
	}

	private void OnCharacterStateChanged()
	{
		if (playerStateModel.CurrentStates.Count == 0)
		{
			cameraState = characterView.GetCameraState(CharacterState.DEFAULT);
		}
		else
		{
			foreach (var item in playerStateModel.CurrentStates)
			{
				if (characterView.HasCameraState(item))
				{
					cameraState = characterView.GetCameraState(item);
					break;
				}
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (characterView != null)
		{
			Gizmos.color = Color.yellow;
			var startPos = characterView.Head.position;
			var startBodyPos = startPos + characterView.Head.up * 0.1f;
			var startHeadPos = startPos + characterView.Head.up * 0f;
			Gizmos.DrawLine(startBodyPos, startBodyPos + characterView.Head.forward);

			Gizmos.color = Color.blue;
			Gizmos.DrawLine(startHeadPos, startHeadPos + characterView.Head.forward);
		}
	}
}
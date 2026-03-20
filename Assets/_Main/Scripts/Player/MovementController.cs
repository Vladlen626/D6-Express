using System;
using _Main.Scripts.Core.Services;
using DG.Tweening;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.UI;
using UnityEngine;

public class MovementController : IBaseController, IActivatable, IUpdatable
{
	private const float CameraBobAmplitude = 0.003f;
	private const float CameraBobFrequency = 7f;
	private const float CameraBobSprintFrequencyMultiplier = 1.2f;
	private const float CameraBobReturnSpeed = 6f;

	private readonly PlayerModel playerModel;
	private readonly CharacterController controller;
	private readonly PlayerView playerView;
	private readonly IInputService inputService;
	private readonly ICursorService cursorService;

	private CameraState cameraState;
	private Vector3 velocity;
	private Vector3 cameraRootBaseLocalPosition;
	private float cameraBobPhase;
	private float cameraBobOffsetY;

	private Vector2 MoveInput => inputService.Move;
	private Vector2 LookInput => inputService.Look;
	private bool IsSprint => inputService.IsSprinting;

	public MovementController(PlayerView playerView, PlayerModel playerModel, IInputService inputService,
		ICursorService cursorService)
	{
		this.playerModel = playerModel;
		this.playerView = playerView;
		this.inputService = inputService;
		this.cursorService = cursorService;
		controller = playerView.CharacterController;
	}

	public void Activate()
	{
		cameraState = playerView.GetCameraState(CharacterState.DEFAULT);
		cameraRootBaseLocalPosition = playerView.CameraRoot.localPosition;
		ResetCameraBob();

		playerModel.PlayerStateModel.StateAdded += OnCharacterStateChanged;
		playerModel.PlayerStateModel.StateRemoved += OnCharacterStateChanged;
	}

	public void Deactivate()
	{
		ResetCameraBob();

		playerModel.PlayerStateModel.StateRemoved -= OnCharacterStateChanged;
		playerModel.PlayerStateModel.StateAdded -= OnCharacterStateChanged;
	}

	public void OnUpdate(float deltaTime)
	{
		if (!playerView.gameObject.activeSelf)
		{
			return;
		}

		if (controller.isGrounded && velocity.y < 0)
		{
			velocity.y = -2f;
		}

		if (controller.enabled)
		{
			Vector3 move = playerView.transform.right * MoveInput.x + playerView.transform.forward * MoveInput.y;
			controller.Move((IsSprint ? playerView.runSpeed : playerView.walkSpeed) * deltaTime * move);

			velocity.y += playerView.gravity * deltaTime;
			controller.Move(velocity * deltaTime);
		}

		UpdateCameraBob(deltaTime);

		var rotationTransform = cameraState.rotationType == RotationType.HEAD ? playerView.Head : playerView.transform;

		float pitch = Mathf.DeltaAngle(0f, playerView.Head.localEulerAngles.x);
		pitch -= LookInput.y * playerView.mouseSensitivity;

		if (cameraState.minPitch != -1 && cameraState.maxPitch != -1)
			pitch = Mathf.Clamp(pitch, cameraState.minPitch, cameraState.maxPitch);

		float yaw = Mathf.DeltaAngle(0f, rotationTransform.localEulerAngles.y);
		yaw += LookInput.x * playerView.mouseSensitivity;

		if (cameraState.minYaw != -1 && cameraState.maxYaw != -1)
			yaw = Mathf.Clamp(yaw, cameraState.minYaw, cameraState.maxYaw);

		if (cameraState.rotationType == RotationType.HEAD)
		{
			rotationTransform.localEulerAngles = new Vector3(pitch, yaw, 0f);
		}
		else
		{
			playerView.Head.localEulerAngles = new Vector3(pitch, 0f, 0f);
			rotationTransform.localEulerAngles = new Vector3(rotationTransform.localEulerAngles.x, yaw, 0f);
		}
	}

	private void OnCharacterStateChanged(CharacterState state)
	{
		ResetCameraBob();

		if (state == CharacterState.DICE_GAME)
		{
			ResetCameraRotation();
		}

		if (playerModel.PlayerStateModel.CurrentStates.Count == 0)
		{
			cameraState = playerView.GetCameraState(CharacterState.DEFAULT);
		}
		else
		{
			foreach (var item in playerModel.PlayerStateModel.CurrentStates)
			{
				if (playerView.HasCameraState(item))
				{
					cameraState = playerView.GetCameraState(item);
					break;
				}
			}
		}
	}

	private void ResetCameraRotation()
	{
		playerView.CameraRoot.DOLocalRotate(Vector3.zero, 0.5f);
	}

	private void UpdateCameraBob(float deltaTime)
	{
		bool isDefaultState = playerModel.PlayerStateModel.CurrentStates.Count == 0;
		bool isMoving = isDefaultState && controller.enabled && controller.isGrounded && MoveInput.sqrMagnitude > 0.01f;
		isMoving = isMoving && playerView.EnableMovementCameraBob;
		float targetOffsetY = 0f;

		if (isMoving)
		{
			float frequency = CameraBobFrequency;
			if (IsSprint)
			{
				frequency *= CameraBobSprintFrequencyMultiplier;
			}

			cameraBobPhase += deltaTime * frequency;
			targetOffsetY = Mathf.Sin(cameraBobPhase) * CameraBobAmplitude;
		}

		cameraBobOffsetY = Mathf.MoveTowards(cameraBobOffsetY, targetOffsetY, CameraBobReturnSpeed * deltaTime);

		var localPosition = cameraRootBaseLocalPosition;
		localPosition.y += cameraBobOffsetY;
		playerView.CameraRoot.localPosition = localPosition;
	}

	private void ResetCameraBob()
	{
		cameraBobPhase = 0f;
		cameraBobOffsetY = 0f;
		playerView.CameraRoot.localPosition = cameraRootBaseLocalPosition;
	}
}

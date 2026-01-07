using System;
using _Main.Scripts.Core.Services;
using DG.Tweening;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.UI;
using UnityEngine;

public class MovementController : IBaseController, IActivatable, IUpdatable
{
	private readonly PlayerModel playerModel;
	private readonly CharacterController controller;
	private readonly PlayerView playerView;
	private readonly IInputService inputService;
	private readonly ICursorService cursorService;

	private CameraState cameraState;
	private Vector3 velocity;

	private Vector2 MoveInput => inputService.Move;
	private Vector2 LookInput => inputService.Look;
	private bool IsSprint => inputService.IsSprinting;
	private Transform Transform => playerView.Body;

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

		playerModel.PlayerStateModel.StateAdded += OnCharacterStateAdded;
		playerModel.PlayerStateModel.StateAdded += OnCharacterStateChanged;
		playerModel.PlayerStateModel.StateRemoved += OnCharacterStateChanged;
		playerModel.PlayerStateModel.StateRemoved += OnCharacterStateRemoved;
		cursorService.LockCursor();
	}

	public void Deactivate()
	{
		playerModel.PlayerStateModel.StateRemoved -= OnCharacterStateRemoved;
		playerModel.PlayerStateModel.StateRemoved -= OnCharacterStateChanged;
		playerModel.PlayerStateModel.StateAdded -= OnCharacterStateChanged;
		playerModel.PlayerStateModel.StateAdded -= OnCharacterStateAdded;

		cursorService.UnlockCursor();
	}

	public void OnUpdate(float deltaTime)
	{
		if (controller.isGrounded && velocity.y < 0)
		{
			velocity.y = -2f;
		}

		if (controller.enabled)
		{
			Vector3 move = Transform.right * MoveInput.x + Transform.forward * MoveInput.y;
			controller.Move((IsSprint ? playerView.runSpeed : playerView.walkSpeed) * deltaTime * move);

			velocity.y += playerView.gravity * deltaTime;
			controller.Move(velocity * deltaTime);
		}

		var rotationTransform = cameraState.rotationType == RotationType.HEAD ? playerView.Head : playerView.Body;

		float pitch = Mathf.DeltaAngle(0f, playerView.CameraRoot.localEulerAngles.x);
		pitch -= LookInput.y * playerView.mouseSensitivity;

		if (cameraState.minPitch != -1 && cameraState.maxPitch != -1)
			pitch = Mathf.Clamp(pitch, cameraState.minPitch, cameraState.maxPitch);

		playerView.CameraRoot.localEulerAngles = new Vector3(pitch, 0f, 0f);

		float yaw = Mathf.DeltaAngle(0f, rotationTransform.localEulerAngles.y);
		yaw += LookInput.x * playerView.mouseSensitivity;

		if (cameraState.minYaw != -1 && cameraState.maxYaw != -1)
			yaw = Mathf.Clamp(yaw, cameraState.minYaw, cameraState.maxYaw);

		rotationTransform.localEulerAngles = new Vector3(0f, yaw, 0f);
	}

	private void OnCharacterStateChanged(CharacterState state)
	{
		if (state == CharacterState.DICE_GAME)
		{
			ResetCameraRotation();
		}
	}

	private void OnCharacterStateAdded(CharacterState state)
	{
		ChangeCameraState(state);
	}

	private void OnCharacterStateRemoved(CharacterState state)
	{
		if (state == cameraState.characterState)
		{
			ChangeCameraState(CharacterState.DEFAULT);
		}
	}

	private void ChangeCameraState(CharacterState state)
	{
		cameraState = playerView.GetCameraState(state);
		playerView.Head.localRotation = Quaternion.identity;
	}

	private void ResetCameraRotation()
	{
		playerView.CameraRoot.DOLocalRotate(Vector3.zero, 0.5f);
	}
}
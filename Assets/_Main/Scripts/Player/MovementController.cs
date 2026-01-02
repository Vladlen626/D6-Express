using _Main.Scripts.Core.Services;
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

	Vector3 velocity;
	Vector2 moveInput => inputService.Move;
	Vector2 lookInput => inputService.Look;
	bool isSprint => inputService.IsSprinting;
	bool isGrounded;
	float rotationX;

	private Transform transform => playerView.transform;

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
		playerModel.OnCharacterStateChanged += OnCharacterStateChanged;
		cursorService.LockCursor();

		inputService.OnLooked += OnLooked;
	}

	public void Deactivate()
	{
		inputService.OnLooked -= OnLooked;
		
		playerModel.OnCharacterStateChanged -= OnCharacterStateChanged;
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
			Vector3 move = playerView.transform.right * moveInput.x + transform.forward * moveInput.y;
			controller.Move((isSprint ? playerView.runSpeed : playerView.walkSpeed) * deltaTime * move);

			velocity.y += playerView.gravity * deltaTime;
			controller.Move(velocity * deltaTime);
		}
	}

	private void OnLooked(Vector2 input)
	{
		rotationX -= input.y * playerView.mouseSensitivity;
		rotationX = Mathf.Clamp(rotationX, -playerView.lookXLimit, playerView.lookXLimit);
		playerView.CameraRoot.localRotation = Quaternion.Euler(rotationX, 0, 0);
		transform.rotation *= Quaternion.Euler(0, input.x * playerView.mouseSensitivity, 0);
	}

	private void OnCharacterStateChanged(CharacterState oldCharacterState, CharacterState newCharacterState)
	{
		if (oldCharacterState == CharacterState.DICE_GAME || newCharacterState == CharacterState.DICE_GAME)
		{
			ResetCameraRotation();
		}
	}

	private void ResetCameraRotation()
	{
		playerView.CameraRoot.localRotation = Quaternion.Euler(0, 0, 0);
	}
}
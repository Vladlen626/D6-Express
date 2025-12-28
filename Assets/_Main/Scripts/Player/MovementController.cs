using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

public class MovementController : IBaseController, IActivatable, IUpdatable
{
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

	public MovementController(PlayerView inPlayerView, IInputService inInputService, ICursorService inCursor)
	{
		playerView = inPlayerView;
		controller = inPlayerView.CharacterController;
		inputService = inInputService;
		cursorService = inCursor;
	}

	public void Activate()
	{
		cursorService.LockCursor();
	}

	public void Deactivate()
	{
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

		rotationX -= lookInput.y * playerView.mouseSensitivity;
		rotationX = Mathf.Clamp(rotationX, -playerView.lookXLimit, playerView.lookXLimit);
		playerView.CameraRoot.localRotation = Quaternion.Euler(rotationX, 0, 0);
		transform.rotation *= Quaternion.Euler(0, lookInput.x * playerView.mouseSensitivity, 0);
	}
}
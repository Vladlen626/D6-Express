using UnityEngine;

public class PlayerView : CharacterView
{
	[SerializeField]
	private CharacterController characterController;

	[Header("Movement")]
	public float walkSpeed = 5f;
	public float runSpeed = 10f;
	public float jumpHeight = 2f;
	public float gravity = -20f;

	[Header("Camera Bob")]
	[SerializeField]
	private bool enableMovementCameraBob = false;
	[SerializeField]
	[Min(0f)]
	private float movementCameraBobAmplitude = 0.003f;
	[SerializeField]
	[Min(0f)]
	private float movementCameraBobFrequency = 7f;
	[SerializeField]
	[Min(0f)]
	private float movementCameraBobSprintFrequencyMultiplier = 1.2f;
	[SerializeField]
	[Min(0f)]
	private float movementCameraBobReturnSpeed = 6f;

	public CharacterController CharacterController => characterController;
	public bool EnableMovementCameraBob => enableMovementCameraBob;
	public float MovementCameraBobAmplitude => movementCameraBobAmplitude;
	public float MovementCameraBobFrequency => movementCameraBobFrequency;
	public float MovementCameraBobSprintFrequencyMultiplier => movementCameraBobSprintFrequencyMultiplier;
	public float MovementCameraBobReturnSpeed => movementCameraBobReturnSpeed;

    protected override void SetGhostActive()
    {
        base.SetGhostActive(); 
		CharacterCollider.enabled = false;
    }

    protected override void SetGhostInactive()
    {
        base.SetGhostInactive();
		CharacterCollider.enabled = true;
    }
}

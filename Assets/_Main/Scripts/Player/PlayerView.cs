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

	public CharacterController CharacterController => characterController;
	public bool EnableMovementCameraBob => enableMovementCameraBob;

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

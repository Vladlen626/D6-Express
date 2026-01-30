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

	public CharacterController CharacterController => characterController;

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
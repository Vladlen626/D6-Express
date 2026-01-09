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

    public override void SetCharacterGhost(bool isGhost)
    {
        base.SetCharacterGhost(isGhost);
		SetCharacterControllerEnabled(!isGhost);
    }

	public void SetCharacterControllerEnabled(bool isEnabled)
	{
		characterController.enabled = isEnabled;
	}
}
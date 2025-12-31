using UnityEngine;

public class PlayerView : MonoBehaviour
{
	[Header("Movement")] 
	public float walkSpeed = 5f;
	public float runSpeed = 10f;
	public float jumpHeight = 2f;
	public float gravity = -20f;

	[SerializeField] 
	private CharacterController characterController;
	
	[SerializeField] 
	private CharacterStateController characterStateController;

	[Header("Look")] 
	public float mouseSensitivity = 2f;
	public float lookXLimit = 45f;
	[SerializeField] private Transform cameraRoot;
	public CharacterController CharacterController => characterController;
	public CharacterStateController CharacterStateController => characterStateController;
	public Transform CameraRoot => cameraRoot;
}
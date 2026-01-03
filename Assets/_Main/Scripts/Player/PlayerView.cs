using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
	[Header("Movement")] 
	public float walkSpeed = 5f;
	public float runSpeed = 10f;
	public float jumpHeight = 2f;
	public float gravity = -20f;
	
	[Header("States")]
	[SerializeReference] [SubclassSelector]
	private List<CharacterStateHandler> states = new();

	[Header("Look")] 
	public float mouseSensitivity = 2f;
	public float lookXLimit = 45f;

	[SerializeField] 
	private CharacterController characterController;
	
	[SerializeField] 
	private Collider characterCollider;

	[SerializeField] 
	private Transform cameraRoot;

	public CharacterStateHandler[] CharacterStateHandlers => states.ToArray();
	public CharacterController CharacterController => characterController;
	public Transform CameraRoot => cameraRoot;

	public void Initialize()
	{
		foreach (var characterStateHandler in states)
		{
			characterStateHandler.Init(this);
		}
	}

	public void SetCharacterGhost(bool isGhost)
	{
		SetColliderEnabled(!isGhost);
		SetCharacterControllerEnabled(!isGhost);
	}

	public void SetColliderEnabled(bool isEnabled)
	{
		characterCollider.enabled = isEnabled;
	}
	
	public void SetCharacterControllerEnabled(bool isEnabled)
	{
		characterController.enabled = isEnabled;
	}
}
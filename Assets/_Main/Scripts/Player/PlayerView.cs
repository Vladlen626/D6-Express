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
	[SerializeReference]
	[SubclassSelector]
	private List<CharacterStateHandler> states = new();

	[Header("Look")]
	public float mouseSensitivity = 2f;

	[SerializeField]
	private Interactor interactor;

	[SerializeField]
	private CharacterController characterController;

	[SerializeField]
	private Collider characterCollider;

	[SerializeField]
	private Transform cameraRoot;

	[SerializeField]
	private Transform head;

	[SerializeField]
	private Transform body;

	[SerializeField]
	private Transform character;

	[SerializeField]
	private CameraState defaultCameraState;

	[SerializeField]
	private CameraState[] cameraStates;

	private readonly Dictionary<CharacterState, CameraState> cameraStatesDict = new();

	public CharacterStateHandler[] CharacterStateHandlers => states.ToArray();
	public CharacterController CharacterController => characterController;
	public Interactor Interactor => interactor;
	public Transform CameraRoot => cameraRoot;
	public Transform Head => head;
	public Transform Body => body;
	public Transform Character => character;

	private void Awake()
	{
		foreach (var item in cameraStates)
		{
			cameraStatesDict.Add(item.characterState, item);
		}
	}

	public void Initialize()
	{
		foreach (var characterStateHandler in states)
		{
			characterStateHandler.Init(this);
		}
	}

	public bool HasCameraState(CharacterState characterState)
	{
		return characterState == CharacterState.DEFAULT || cameraStatesDict.ContainsKey(characterState);
	}

	public CameraState GetCameraState(CharacterState characterState)
	{
		if (characterState == CharacterState.DEFAULT || !cameraStatesDict.ContainsKey(characterState))
		{
			return defaultCameraState;
		}

		return cameraStatesDict[characterState];
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

public enum RotationType
{
	BODY,
	HEAD
}

[Serializable]
public struct CameraState
{
	public CharacterState characterState;
	public RotationType rotationType;
	public float minPitch;
	public float maxPitch;
	public float minYaw;
	public float maxYaw;
}
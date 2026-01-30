using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;

public class CharacterView : MonoBehaviour
{
	[Header("Name")]
	[SerializeField]
	private string characterName;

	[Header("States")]
	[SerializeReference]
	[SubclassSelector]
	private List<CharacterStateHandler> states = new();

	[Header("Look")]
	public float mouseSensitivity = 2f;

	[SerializeField]
	private Interactor interactor;

	[SerializeField]
	private Collider characterCollider;

	[SerializeField]
	private Transform cameraRoot;

	[SerializeField]
	private Transform head;

	[SerializeField]
	private CameraState defaultCameraState;

	[SerializeField]
	private CameraState[] cameraStates;

	[SerializeField]
	private Animator animator;

	private readonly Dictionary<CharacterState, CameraState> cameraStatesDict = new();

	private int ghostCount;

	public Animator Animator => animator;
	public CharacterStateHandler[] CharacterStateHandlers => states.ToArray();
	public IReadOnlyList<CameraState> CameraStates => cameraStates;

	protected Collider CharacterCollider => characterCollider;
	public Transform CameraRoot => cameraRoot;
	public Transform Head => head;
	public Interactor Interactor => interactor;
	public string CharacterName => characterName;

	private void Awake()
	{
		foreach (var item in cameraStates)
		{
			cameraStatesDict.Add(item.characterState, item);
		}

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
		int prev = ghostCount;

		if (isGhost)
		{
			ghostCount++;
		}
		else
		{
			ghostCount = Math.Max(0, ghostCount - 1);
		}

		if (prev == 0 && ghostCount > 0)
		{
			SetGhostActive();
		}
		else if (prev > 0 && ghostCount == 0)
		{
			SetGhostInactive();
		}
	}

	protected virtual void SetGhostActive()
	{
		CharacterCollider.enabled = false;
	}

	protected virtual void SetGhostInactive()
	{
		CharacterCollider.enabled = true;
	}
}

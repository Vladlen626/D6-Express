using System.Collections.Generic;
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

	private readonly Dictionary<CharacterState, CameraState> cameraStatesDict = new();

	public CharacterStateHandler[] CharacterStateHandlers => states.ToArray();

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

	public void SetColliderEnabled(bool isEnabled)
	{
		CharacterCollider.enabled = isEnabled;
	}

	public virtual void SetCharacterGhost(bool isGhost)
	{
		SetColliderEnabled(!isGhost);
	}
}

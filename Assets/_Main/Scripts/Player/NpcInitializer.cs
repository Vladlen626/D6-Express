using UnityEngine;

// todo: эта хуйня временна
// надо перенести на фабрики, создания из рута и т.д.
// господи прости
public class NpcInitializer : MonoBehaviour
{
	[SerializeField]
	private Interactable interactable;

	private NpcRotationController npcRotationController;
	public PlayerStateModel playerStateModel;

	void Awake()
	{
		playerStateModel = new();

		var npcView = GetComponent<NpcView>();

		var interactor = GetComponent<Interactor>();
		interactor.Initialize(playerStateModel);

		npcRotationController = GetComponent<NpcRotationController>();
		npcRotationController.Initialize(npcView, playerStateModel);
	}

	void Start()
	{
		var npcView = GetComponent<NpcView>();
		playerStateModel.FillCharacterStatesDict(npcView.CharacterStateHandlers);

		if (interactable != null)
		{
			var interactor = GetComponent<Interactor>();
			interactor.Interact(interactable);
		}
	}
}
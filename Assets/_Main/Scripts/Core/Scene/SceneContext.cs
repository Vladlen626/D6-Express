using UnityEngine;

public class SceneContext : MonoBehaviour, ISceneContext
{
	[SerializeField]
	private DiceTableView diceGameTableView;

	[SerializeField]
	private Transform playerSpawnPosition;

	[SerializeField]
	private InteractorView interactorView;

	[SerializeField]
	private LevelView levelView;

	[SerializeField]
	private SleepView sleepView;

	public DiceTableView DiceGameTableView => diceGameTableView;
	public Transform PlayerSpawnPosition => playerSpawnPosition;
	public InteractorView InteractorView => interactorView;
	public LevelView LevelView => levelView;
	public SleepView SleepView => sleepView;
}

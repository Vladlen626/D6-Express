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
	private Light sun;

	[SerializeField]
	private SleepView sleepView;

	public DiceTableView DiceGameTableView => diceGameTableView;
	public Transform PlayerSpawnPosition => playerSpawnPosition;
	public InteractorView InteractorView => interactorView;
	public Light Sun => sun;
	public SleepView SleepView => sleepView;
}

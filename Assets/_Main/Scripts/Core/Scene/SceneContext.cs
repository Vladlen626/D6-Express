using UnityEngine;

public class SceneContext : MonoBehaviour, ISceneContext
{
	[SerializeField]
	private DiceTableView diceGameTableView;

	[SerializeField]
	private Transform playerTrainSpawnPosition;

	[SerializeField]
	private Transform playerStationSpawnPosition;

	[SerializeField]
	private GameObject stationBlock;

	[SerializeField]
	private GameObject trainBlock;

	[SerializeField]
	private InteractorView interactorView;

	[SerializeField]
	private Light sun;

	public DiceTableView DiceGameTableView => diceGameTableView;
	public Transform PlayerTrainSpawnPosition => playerTrainSpawnPosition;
	public Transform PlayerStationSpawnPosition => playerStationSpawnPosition;
	public GameObject StationBlock => stationBlock;
	public GameObject TrainBlock => trainBlock;
	public InteractorView InteractorView => interactorView;
	public Light Sun => sun;
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
	private Light sun;

	[SerializeField]
	private ShopView shop;

	[SerializeField]
	private CharacterView shopkeeper;

	[SerializeField]
	private InformationPanelView informationPanelView;

	public DiceTableView DiceGameTableView => diceGameTableView;
	public Transform PlayerTrainSpawnPosition => playerTrainSpawnPosition;
	public Transform PlayerStationSpawnPosition => playerStationSpawnPosition;
	public GameObject StationBlock => stationBlock;
	public GameObject TrainBlock => trainBlock;
	public Light Sun => sun;
	public ShopView Shop => shop;

	public CharacterView Shopkeeper => shopkeeper;
	public InformationPanelView InformationPanelView => informationPanelView;


	public IEnumerable<LightView> Lights => FindObjectsByType<LightView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
	public IEnumerable<SpawnPoint> SpawnPoints => FindObjectsByType<SpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
}
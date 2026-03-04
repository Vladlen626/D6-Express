using System.Collections.Generic;
using UnityEngine;

public class SceneContext : MonoBehaviour, ISceneContext
{
	[SerializeField]
	private DiceTableView diceGameTableView;

	[SerializeField] 
	private CombinationsView combinationsView;

	[SerializeField]
	private InventoryView inventoryView;

	[SerializeField]
	private Transform playerTrainSpawnPosition;

	[SerializeField]
	private Transform playerStationSpawnPosition;

	[SerializeField]
	private GameObject stationBlock;

	[SerializeField]
	private GameObject trainBlock;

	[SerializeField]
	private GameObject mainMenuBlock;

	[SerializeField]
	private Light sun;

	[SerializeField]
	private ShopView stationShop;

	[SerializeField]
	private ShopView trainShop;

	[SerializeField]
	private CharacterView stationShopkeeper;

	[SerializeField]
	private CharacterView trainShopkeeper;

	[SerializeField]
	private InformationPanelView informationPanelView;

	// todo: должно создаваться в гейм руте
	// но сейчас npcinitializer не умеет смотреть не на сцену
	// уйдет отсюда когда урмет npcinitializer
	private InteractionToStateTable interactionToStateTable = InteractionFactory.CreateTable();

	public InteractionToStateTable InteractionToStateTable => interactionToStateTable;
	public DiceTableView DiceGameTableView => diceGameTableView;
	public CombinationsView CombinationsView => combinationsView;
	public Transform PlayerTrainSpawnPosition => playerTrainSpawnPosition;
	public Transform PlayerStationSpawnPosition => playerStationSpawnPosition;
	public GameObject StationBlock => stationBlock;
	public GameObject TrainBlock => trainBlock;
	public GameObject MainMenuBlock => mainMenuBlock;
	public Light Sun => sun;
	public ShopView StationShop => stationShop;
	public ShopView TrainShop => trainShop;

	public InventoryView InventoryView => inventoryView;

	public CharacterView StationShopkeeper => stationShopkeeper;
	public CharacterView TrainShopkeeper => trainShopkeeper;
	public InformationPanelView InformationPanelView => informationPanelView;


	public IEnumerable<LedTrainView> Leds => FindObjectsByType<LedTrainView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
	public IEnumerable<LightView> Lights => FindObjectsByType<LightView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
	public IEnumerable<SpawnPoint> SpawnPoints => FindObjectsByType<SpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
}
using _Main.Scripts.Core.Services;
using _Main.Scripts.UI;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;

public static class PlayerFactory
{
	public static async UniTask<PlayerModel> CreatePlayerModel(ConfigService configService)
	{
		var playerModel = new PlayerModel();

		var playerConfig = await configService.GetFirstOrDefaultAsync<PlayerConfig>(ResourcePaths.Json.player);
		int startCash = playerConfig.cash;
	
		playerModel.InventoryModel.GiveCash(startCash);

		foreach (var playerConfigDice in playerConfig.dices)
		{
			playerModel.InventoryModel.AddDice(playerConfigDice);
		}


		return playerModel;
	}
	
	
	public static async UniTask<PlayerView> SpawnPlayerView(
		SceneContext sceneContext,
		IObjectFactory factory,
		IInputService inputService,
		PlayerModel playerModel)
	{
		var playerView = await factory.CreateAsync<PlayerView>(ResourcePaths.Player.PlayerPrefab, sceneContext.PlayerTrainSpawnPosition.position,
			sceneContext.PlayerTrainSpawnPosition.rotation);

		var playerInteractSystem = playerView.GetComponent<Interactor>();
		playerInteractSystem.Initialize(inputService, playerModel.PlayerStateModel);
		sceneContext?.InteractorView.Initialize(playerInteractSystem);

		return playerView;
	}

	public static IBaseController[] GetPlayerBaseControllers(PlayerView playerView, ServiceLocator serviceLocator,
		PlayerModel playerModel)
	{
		var input = serviceLocator.Get<IInputService>();
		var cursor = serviceLocator.Get<ICursorService>();
		var uiService = serviceLocator.Get<IUIService>();

		var playerControllers = new IBaseController[]
		{
			new MovementController(playerView, playerModel, input, cursor),
			new PlayerHudController(uiService, playerModel),
		};

		return playerControllers;
	}
}
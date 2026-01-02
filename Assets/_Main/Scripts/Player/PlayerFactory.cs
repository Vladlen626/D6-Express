using _Main.Scripts.Core.Services;
using _Main.Scripts.UI;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;

public static class PlayerFactory
{
	public static PlayerModel CreatePlayerModel()
	{
		// TODO: Перенести в конфиги
		int startCash = 300;
		var playerModel = new PlayerModel();
		playerModel.InventoryModel.GiveCash(startCash);
		
		
		return playerModel;
	}
	
	
	public static async UniTask<PlayerView> SpawnPlayerView(SceneContext sceneContext, IObjectFactory factory, IInputService inputService)
	{
		var playerView =  await factory.CreateAsync<PlayerView>(ResourcePaths.Player.PlayerPrefab, sceneContext.PlayerTrainSpawnPosition.position,
			sceneContext.PlayerTrainSpawnPosition.rotation);

		var playerInteractSystem = playerView.GetComponent<Interactor>();
		playerInteractSystem.Initialize(inputService);
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
			new PlayerHudController(uiService, playerModel)
		};
		
		return playerControllers;
	}
}
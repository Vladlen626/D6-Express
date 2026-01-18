using _Main.Scripts.Core.Services;
using _Main.Scripts.UI;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Audio;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;
using UnityEngine;

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
		IObjectFactory factory,
		IInputService inputService,
		PlayerModel playerModel,
		Transform spawnTfm)
	{
		var playerView = await factory.CreateAsync<PlayerView>(ResourcePaths.Player.PlayerBase, spawnTfm.position,
			spawnTfm.rotation);

		var playerInteractSystem = playerView.GetComponent<InteractorPlayer>();
		playerInteractSystem.Initialize(inputService, playerModel.PlayerStateModel);

		return playerView;
	}

	public static IBaseController[] GetPlayerBaseControllers(PlayerView playerView, ServiceLocator serviceLocator,
		PlayerModel playerModel, IInputService inputService, IAudioService audioService)
	{
		var input = serviceLocator.Get<IInputService>();
		var cursor = serviceLocator.Get<ICursorService>();
		var uiService = serviceLocator.Get<IUIService>();

		var fartView = playerView.GetComponent<FartView>();

		var playerControllers = new IBaseController[]
		{
			new MovementController(playerView, playerModel, input, cursor),
			new PlayerHudController(uiService, playerModel),
			new HintController(uiService, playerView),
			new FartController(fartView, inputService, audioService)
		};

		return playerControllers;
	}
}
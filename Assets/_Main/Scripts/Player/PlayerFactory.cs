using System.Collections.Generic;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services;
using PlatformCore.Services.Factory;

public static class PlayerFactory
{

	public static async UniTask<PlayerView> SpawnPlayer(SceneContext sceneContext, IObjectFactory factory)
	{
		return await factory.CreateAsync<PlayerView>(ResourcePaths.Player.PlayerPrefab, sceneContext.PlayerSpawnPosition.position,
			sceneContext.PlayerSpawnPosition.rotation);;
	}
	
	public static IBaseController[] GetPlayerBaseControllers(PlayerView playerView, ServiceLocator serviceLocator)
	{
		var input = serviceLocator.Get<IInputService>();
		var cursor = serviceLocator.Get<ICursorService>();
		
		var playerControllers = new IBaseController[]
		{
			new MovementController(playerView, input, cursor),
		};
		
		return playerControllers;
	}
}
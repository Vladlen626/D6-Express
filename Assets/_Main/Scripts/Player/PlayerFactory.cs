using System.Collections.Generic;
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
	
	public static async UniTask<IBaseController[]> GetPlayerBaseControllers(SceneContext sceneContext, IObjectFactory factory)
	{
		var playerControllers = new List<IBaseController>();
		return playerControllers.ToArray();
	}
}
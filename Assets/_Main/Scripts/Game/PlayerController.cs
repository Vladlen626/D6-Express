using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;

public class PlayerController : IBaseController, IGameStateChanger
{
	private readonly PlayerModel playerModel;
	private readonly PlayerView playerView;
	private readonly SceneContext sceneContext;

	public PlayerController(PlayerModel playerModel, PlayerView playerView, SceneContext sceneContext)
	{
		this.playerModel = playerModel;
		this.playerView = playerView;
		this.sceneContext = sceneContext;
	}

	public IEnumerable<(GameStateTransitionTask task, GameStateChangeFunc func)> GetStateChangeFuncs()
	{
		yield return (GameStateTransitionTask.CHARACTER_TRANSITION_START, (x) => StartChangeState(x));
		yield return (GameStateTransitionTask.CHARACTER_TRANSITION_FINISH, (x) => StopChangeState(x));
		yield return (GameStateTransitionTask.CHANGE_LOCATION, (x) => ChangeLocation(x));
	}

	private async UniTask StartChangeState(GameStateTransition data)
	{
		playerView.Interactor.StopAllActions(true);
		playerModel.PlayerStateModel.TryAddState(CharacterState.TRANSITION);
		playerView.SetCharacterGhost(true);
	}

	private async UniTask StopChangeState(GameStateTransition data)
	{
		playerView.SetCharacterGhost(false);
		playerModel.PlayerStateModel.TryRemoveState(CharacterState.TRANSITION);
	}

	private async UniTask ChangeLocation(GameStateTransition data)
	{
		if (data.Location == Location.STATION)
		{
			playerView.transform.SetPositionAndRotation(sceneContext.PlayerStationSpawnPosition.position,
				sceneContext.PlayerStationSpawnPosition.rotation);
		}
		else if (data.Location == Location.TRAIN)
		{
			playerView.transform.SetPositionAndRotation(sceneContext.PlayerTrainSpawnPosition.position,
				sceneContext.PlayerTrainSpawnPosition.rotation);
		}
	}
}
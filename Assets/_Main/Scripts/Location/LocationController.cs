using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Audio;

public class LocationController : IBaseController, IGameStateChanger
{
	private readonly D6Game game;
	private readonly Run run;
	private readonly SceneContext sceneContext;
	private readonly IAudioService audioService;
	private readonly PlayerModel playerModel;
	private readonly PlayerView playerView;

	public LocationController(D6Game game, Run run, SceneContext sceneContext, IAudioService audioService, PlayerModel playerModel, PlayerView playerView)
	{
		this.game = game;
		this.run = run;
		this.sceneContext = sceneContext;
		this.audioService = audioService;
		this.playerModel = playerModel;
		this.playerView = playerView;
	}

	public IEnumerable<(GameStateTransitionTask task, GameStateChangeFunc func)> GetStateChangeFuncs()
	{
		yield return (GameStateTransitionTask.CHANGE_LOCATION, (x) => Perform(x));
	}

	private UniTask Perform(GameStateTransition data)
	{
		playerModel.PlayerStateModel.TryAddState(CharacterState.LOCATION_TRANSITIONING);
		playerView.SetCharacterGhost(true);

		sceneContext.TrainBlock.SetActive(data.Location == Location.TRAIN);
		sceneContext.StationBlock.SetActive(data.Location == Location.STATION);
		sceneContext.MainMenuBlock.SetActive(data.Location == Location.MAIN_MENU);

		if (data.Location == Location.STATION)
		{
			audioService.StopParallelSound(SoundNames.TrainSound);
			audioService.PlaySoundParallel(SoundNames.StationSound);
			playerView.transform.SetPositionAndRotation(sceneContext.PlayerStationSpawnPosition.position,
				sceneContext.PlayerStationSpawnPosition.rotation);
		}
		else
		{
			audioService.StopParallelSound(SoundNames.StationSound);
			audioService.PlaySoundParallel(SoundNames.TrainSound);
			playerView.transform.SetPositionAndRotation(sceneContext.PlayerTrainSpawnPosition.position,
				sceneContext.PlayerStationSpawnPosition.rotation);
		}

		game.SetLocation(data.Location.Value);

		playerView.SetCharacterGhost(false);
		playerModel.PlayerStateModel.TryRemoveState(CharacterState.LOCATION_TRANSITIONING);

		return UniTask.CompletedTask;
	}
}
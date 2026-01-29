using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Audio;

public class LocationController : IBaseController, IGameStateChanger
{
	private readonly D6Game game;
	private readonly SceneContext sceneContext;
	private readonly IAudioService audioService;

	public LocationController(D6Game game, SceneContext sceneContext, IAudioService audioService)
	{
		this.game = game;
		this.sceneContext = sceneContext;
		this.audioService = audioService;
	}

	public IEnumerable<(GameStateTransitionTask task, GameStateChangeFunc func)> GetStateChangeFuncs()
	{
		yield return (GameStateTransitionTask.CHANGE_LOCATION, (x) => Perform(x));
	}

	private UniTask Perform(GameStateTransition data)
	{
		sceneContext.TrainBlock.SetActive(data.Location == Location.TRAIN);
		sceneContext.StationBlock.SetActive(data.Location == Location.STATION);
		sceneContext.MainMenuBlock.SetActive(data.Location == Location.MAIN_MENU);

		if (data.Location == Location.STATION)
		{
			audioService.StopParallelSound(SoundNames.TrainSound);
			audioService.PlaySoundParallel(SoundNames.StationSound);
		}
		else
		{
			audioService.StopParallelSound(SoundNames.StationSound);
			audioService.PlaySoundParallel(SoundNames.TrainSound);
		}

		game.SetLocation(data.Location.Value);

		return UniTask.CompletedTask;
	}
}
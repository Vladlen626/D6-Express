using _Main.Scripts.Dice;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Audio;
using UnityEngine;

public class LevelController : IBaseController, IActivatable
{
	private const string StationSound = "event:/StationSound";
	private const string TrainSound = "event:/TrainSound";
	
	private readonly RunModel runModel;
	private readonly PlayerModel playerModel;
	private readonly DiceGameModel diceGameModel;
	private readonly PlayerView playerView;
	private readonly Transform playerTrainSpawnPosition;
	private readonly Transform playerStationSpawnPosition;
	private readonly IAudioService audioService;

	public LevelController(RunModel runModel, PlayerModel playerModel, DiceGameModel diceGameModel,
		PlayerView playerView,
		Transform playerTrainSpawnPosition,
		Transform playerStationSpawnPosition,
		IAudioService audioService)
	{
		this.runModel = runModel;
		this.playerModel = playerModel;
		this.diceGameModel = diceGameModel;
		this.playerView = playerView;
		this.playerTrainSpawnPosition = playerTrainSpawnPosition;
		this.playerStationSpawnPosition = playerStationSpawnPosition;
		this.audioService = audioService;
	}

	public void Activate()
	{
		runModel.StateChanged += OnLevelStateChanged;
		runModel.LevelModel.OnFinalDay += OnFinalDayHandler;

		OnLevelStateChanged();
	}

	public void Deactivate()
	{
		runModel.StateChanged -= OnLevelStateChanged;
		runModel.LevelModel.OnFinalDay -= OnFinalDayHandler;
	}

	private void OnFinalDayHandler()
	{
		runModel.LevelModel.SetLevelFinished(playerModel.InventoryModel.CashCount >= runModel.LevelModel.CashGoal);
	}

	private void OnLevelStateChanged()
	{
		playerModel.PlayerStateModel.TryAddState(CharacterState.LOCATION_TRANSITIONING);

		if (runModel.LevelState == LevelState.STATION)
		{
			audioService.StopParallelSound(TrainSound);
			audioService.PlaySoundParallel(StationSound);
			playerView.SetCharacterGhost(true);
			playerView.transform.SetPositionAndRotation(playerStationSpawnPosition.position,
				playerStationSpawnPosition.rotation);
			playerView.SetCharacterGhost(false);
		}
		else
		{
			audioService.StopParallelSound(StationSound);
			audioService.PlaySoundParallel(TrainSound);
			playerView.SetCharacterGhost(true);
			playerView.transform.SetPositionAndRotation(playerTrainSpawnPosition.position,
				playerTrainSpawnPosition.rotation);
			playerView.SetCharacterGhost(false);
		}

		playerModel.PlayerStateModel.TryRemoveState(CharacterState.LOCATION_TRANSITIONING);
	}
}
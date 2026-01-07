using _Main.Scripts.Dice;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

public class LevelController : IBaseController, IActivatable
{
	private readonly RunModel runModel;
	private readonly PlayerModel playerModel;
	private readonly DiceGameModel diceGameModel;
	private readonly PlayerView playerView;
    private readonly Transform playerTrainSpawnPosition;
    private readonly Transform playerStationSpawnPosition;

    public LevelController(RunModel runModel, PlayerModel playerModel, DiceGameModel diceGameModel, PlayerView playerView, Transform playerTrainSpawnPosition,
		Transform playerStationSpawnPosition)
	{
		this.runModel = runModel;
		this.playerModel = playerModel;
		this.diceGameModel = diceGameModel;
		this.playerView = playerView;
        this.playerTrainSpawnPosition = playerTrainSpawnPosition;
        this.playerStationSpawnPosition = playerStationSpawnPosition;
    }

	public void Activate()
	{
		runModel.StateChanged += OnLevelStateChanged;
		runModel.LevelModel.LevelFinished += DayLevelFinishedHandler;
		runModel.LevelModel.OnFinalDay += OnFinalDayHandler;

		OnLevelStateChanged();
	}

	public void Deactivate()
	{
		runModel.StateChanged -= OnLevelStateChanged;
		runModel.LevelModel.LevelFinished -= DayLevelFinishedHandler;
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
			playerView.SetCharacterGhost(true);
			playerView.Character.SetPositionAndRotation(playerStationSpawnPosition.position,
				playerStationSpawnPosition.rotation);
			playerView.SetCharacterGhost(false);
		}
		else
		{
			playerView.SetCharacterGhost(true);
			playerView.Character.SetPositionAndRotation(playerTrainSpawnPosition.position,
				playerTrainSpawnPosition.rotation);
			playerView.SetCharacterGhost(false);
		}

		playerModel.PlayerStateModel.TryRemoveState(CharacterState.LOCATION_TRANSITIONING);
	}

	private void DayLevelFinishedHandler(bool result)
	{
		if (result)
		{
			Debug.Log("YOU WIN");
		}
		else
		{
			Debug.Log("YOU LOSE");
		}
	}
}
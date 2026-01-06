using _Main.Scripts.Dice;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

public class LevelController : IBaseController, IActivatable
{
	private readonly RunModel runModel;
	private readonly PlayerModel playerModel;
	private readonly DiceGameModel diceGameModel;

	public LevelController(RunModel runModel, PlayerModel playerModel, DiceGameModel diceGameModel)
	{
		this.runModel = runModel;
		this.playerModel = playerModel;
		this.diceGameModel = diceGameModel;
	}

	public void Activate()
	{
		runModel.StateChanged += OnLevelStateChanged;
		runModel.LevelModel.LevelFinished += DayLevelFinishedHandler;
		runModel.LevelModel.OnFinalDay += OnFinalDayHandler;
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

		switch (runModel.LevelState)
		{
			case LevelState.STATION:
				break;
			case LevelState.TRAIN:
				break;
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
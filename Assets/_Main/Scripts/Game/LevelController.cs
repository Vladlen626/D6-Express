using _Main.Scripts.Dice;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

public class LevelController : IBaseController, IActivatable
{
	private readonly LevelModel levelModel;
	private readonly PlayerModel playerModel;
	private readonly DiceGameModel diceGameModel;

	private InventoryModel inventoryModel => playerModel.InventoryModel;

	public LevelController(LevelModel levelModel, PlayerModel playerModel, DiceGameModel diceGameModel)
	{
		this.levelModel = levelModel;
		this.playerModel = playerModel;
		this.diceGameModel = diceGameModel;
	}

	public void Activate()
	{
		levelModel.DayChanged += DayChangedHandler;
		diceGameModel.OnGameConditionPassed += OnDiceGameConditionPassedHandler;
		diceGameModel.OnGameConditionFailed += OnDiceGameConditionFailedHandler;
		levelModel.LevelStateChanged += OnLevelStateChanged;
	}

	public void Deactivate()
	{
		levelModel.LevelStateChanged -= OnLevelStateChanged;
		levelModel.DayChanged -= DayChangedHandler;
		diceGameModel.OnGameConditionPassed -= OnDiceGameConditionPassedHandler;
		diceGameModel.OnGameConditionFailed += OnDiceGameConditionFailedHandler;
	}

	private void OnDiceGameConditionPassedHandler()
	{
		inventoryModel.GiveCash(diceGameModel.BetSize);
	}

	private void OnDiceGameConditionFailedHandler()
	{
		inventoryModel.TakeCash(diceGameModel.BetSize);
	}

	private void OnLevelStateChanged()
	{
		switch (levelModel.LevelState)
		{
			case LevelState.STATION:
				
				break;
			case LevelState.TRAIN:

				break;
		}
	}

	private void DayChangedHandler()
	{
		if (inventoryModel.CashCount >= levelModel.CashGoal)
		{
			Debug.Log("YOU WIN");
		}
		else
		{
			Debug.Log("YOU LOSE");
		}
	}
}
using _Main.Scripts.Dice;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

public class LevelController : IBaseController, IActivatable
{
	private readonly LevelModel levelModel;
	private readonly PlayerModel playerModel;
	private readonly DiceGameModel diceGameModel;

	public LevelController(LevelModel levelModel, PlayerModel playerModel, DiceGameModel diceGameModel)
	{
		this.levelModel = levelModel;
		this.playerModel = playerModel;
		this.diceGameModel = diceGameModel;
	}

	public void Activate()
	{
		levelModel.LevelStateChanged += OnLevelStateChanged;
		levelModel.LevelFinished += DayLevelFinishedHandler;
	}

	public void Deactivate()
	{
		levelModel.LevelStateChanged -= OnLevelStateChanged;
		levelModel.LevelFinished -= DayLevelFinishedHandler;
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
using System;
using PlatformCore.Core;

public struct LevelData
{
	public int cashGoal;
	public int days;
	public int ticks;
}

public class RunModel
{
	private LevelData[] levelData;

	private LevelState state = LevelState.STATION;

	public int LevelIndex { get; private set; }
	public int MaxLevels => levelData.Length;
	public LevelModel LevelModel { get; private set; } = new();
	public LevelState LevelState => state;

	public event Action StateChanged;
	public event Action<bool> Finished;

	public RunModel()
	{
		LevelModel.LevelFinished += OnLevelFinished;
	}

	public void UpdateRun(LevelData[] levelData)
	{
		this.levelData = levelData;

		var currentLevelData = levelData[LevelIndex];
		LevelModel.UpdateLevel(currentLevelData, GetTicketPrice(LevelIndex));
	}

	public void SetLevelState(LevelState state)
	{
		this.state = state;
		StateChanged?.Invoke();
	}

	private void OnLevelFinished(bool result)
	{
		if (result)
		{
			LevelIndex++;
			if (LevelIndex >= levelData.Length)
			{
				Finished?.Invoke(true);
			}
			else
			{
				var currentLevelData = levelData[LevelIndex];
				SetLevelState(LevelState.STATION);
				LevelModel.UpdateLevel(currentLevelData, GetTicketPrice(LevelIndex));
			}
		}
		else
		{
			Finished?.Invoke(false);
		}
	}

	private int GetTicketPrice(int levelIndex)
	{
		if (levelIndex == 0)
		{
			return 0;
		}
		else
		{
			return levelData[levelIndex - 1].cashGoal;
		}
	}
}

using PlatformCore.Core;
using PlatformCore.Services.UI;
using UnityEngine;

public class LevelViewController : BaseContextController<UILevelView>
{
	private readonly LevelModel levelModel;
	private readonly Light sun;
	private readonly GameObject trainBlock;
	private readonly GameObject stationBlock;
	private readonly PlayerView playerView;
	private readonly Transform playerTrainSpawnPosition;
	private readonly Transform playerStationSpawnPosition;

	public LevelViewController(IUIService uiService, LevelModel levelModel, Light sun, GameObject trainBlock,
		GameObject stationBlock, PlayerView playerView, Transform playerTrainSpawnPosition,
		Transform playerStationSpawnPosition) : base(uiService)
	{
		this.levelModel = levelModel;
		this.sun = sun;
		this.trainBlock = trainBlock;
		this.stationBlock = stationBlock;
		this.playerView = playerView;
		this.playerTrainSpawnPosition = playerTrainSpawnPosition;
		this.playerStationSpawnPosition = playerStationSpawnPosition;
	}

	protected override void OnActivate()
	{
		levelModel.TickChanged += OnTickChanged;
		levelModel.DayChanged += OnDaysChanged;
		levelModel.LevelStateChanged += OnLevelStateChanged;

		OnTickChanged();
		OnDaysChanged();
		OnLevelStateChanged();
	}

	protected override void OnDeactivate()
	{
		levelModel.LevelStateChanged -= OnLevelStateChanged;
		levelModel.DayChanged -= OnDaysChanged;
		levelModel.TickChanged -= OnTickChanged;
	}

	private void OnTickChanged()
	{
		RotateSun();

		_context.SetTicksText($"Ticks: {levelModel.Tick} / {levelModel.TicksPerDay}");
	}

	private void OnLevelStateChanged()
	{
		trainBlock.SetActive(levelModel.LevelState == LevelState.TRAIN);
		stationBlock.SetActive(levelModel.LevelState == LevelState.STATION);

		if (levelModel.LevelState == LevelState.STATION)
		{
			playerView.SetCharacterGhost(true);
			playerView.transform.SetPositionAndRotation(playerStationSpawnPosition.position,
				playerStationSpawnPosition.rotation);
			playerView.SetCharacterGhost(false);
		}
		else
		{
			playerView.SetCharacterGhost(true);
			playerView.transform.SetPositionAndRotation(playerTrainSpawnPosition.position,
				playerTrainSpawnPosition.rotation);
			playerView.SetCharacterGhost(false);
		}
	}

	private void RotateSun()
	{
		var currentTickRatio = _context.RatioModifier.Evaluate(levelModel.TickRatio);

		sun.transform.rotation = Quaternion.Euler(currentTickRatio * 360f - 90f, 170f, 0f);
		sun.color = _context.LightColor.Evaluate(currentTickRatio);
		sun.intensity = _context.LightIntensity.Evaluate(currentTickRatio);
	}

	private void OnDaysChanged()
	{
		_context.SetDaysText($"Days: {levelModel.Day} / {levelModel.Days}");
	}
}
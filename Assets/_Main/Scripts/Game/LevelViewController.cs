using PlatformCore.Core;
using PlatformCore.Services.UI;
using UnityEngine;

public class LevelViewController : BaseContextController<UILevelView>
{
	private readonly PlayerModel playerModel;
	private readonly RunModel runModel;
	private readonly Light sun;
	private readonly GameObject trainBlock;
	private readonly GameObject stationBlock;

	public LevelViewController(IUIService uiService, PlayerModel playerModel, RunModel runModel, Light sun, GameObject trainBlock,
		GameObject stationBlock) : base(uiService)
	{
		this.playerModel = playerModel;
		this.runModel = runModel;
		this.sun = sun;
		this.trainBlock = trainBlock;
		this.stationBlock = stationBlock;
	}

	protected override void OnActivate()
	{
		runModel.LevelModel.TickChanged += OnTickChanged;
		runModel.LevelModel.DayChanged += OnDaysChanged;
		runModel.StateChanged += OnLevelStateChanged;

		playerModel.InventoryModel.OnCashCountChanged += OnCashChanged;

		OnCashChanged();
		OnTickChanged();
		OnDaysChanged();
		OnLevelStateChanged();
	}

	protected override void OnDeactivate()
	{
		playerModel.InventoryModel.OnCashCountChanged -= OnCashChanged;

		runModel.StateChanged -= OnLevelStateChanged;
		runModel.LevelModel.DayChanged -= OnDaysChanged;
		runModel.LevelModel.TickChanged -= OnTickChanged;
	}

	private void OnTickChanged()
	{
		RotateSun();

		_context.SetTicksText($"Ticks: {runModel.LevelModel.Tick + 1} / {runModel.LevelModel.Ticks}");
	}

	private void OnLevelStateChanged()
	{
		_context.SetRunText($"Run: {runModel.LevelIndex + 1} / {runModel.MaxLevels}");

		trainBlock.SetActive(runModel.LevelState == LevelState.TRAIN);
		stationBlock.SetActive(runModel.LevelState == LevelState.STATION);
	}

	private void RotateSun()
	{
		var currentTickRatio = _context.RatioModifier.Evaluate(runModel.LevelModel.TickRatio);

		sun.transform.rotation = Quaternion.Euler(currentTickRatio * 360f - 90f, 170f, 0f);
		sun.color = _context.LightColor.Evaluate(currentTickRatio);
		sun.intensity = _context.LightIntensity.Evaluate(currentTickRatio);
	}

	private void OnDaysChanged()
	{
		_context.SetDaysText($"Days: {runModel.LevelModel.Day + 1} / {runModel.LevelModel.Days}");
	}

	private void OnCashChanged()
	{
		_context.SetCashProgress($"Progress: {playerModel.InventoryModel.CashCount}$ / {runModel.LevelModel.CashGoal}$");
	}
}
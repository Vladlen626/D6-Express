using PlatformCore.Core;
using PlatformCore.Services.UI;
using UnityEngine;

public class LevelViewController : BaseContextController<UILevelView>
{
	private readonly PlayerModel playerModel;
	private readonly RunModel runModel;
	private readonly Light sun;

	public LevelViewController(IUIService uiService, PlayerModel playerModel, RunModel runModel, Light sun) : base(uiService)
	{
		this.playerModel = playerModel;
		this.runModel = runModel;
		this.sun = sun;
	}

	protected override void OnActivate()
	{
		runModel.LevelModel.TickChanged += OnTickChanged;
		runModel.LevelModel.DayChanged += OnDaysChanged;

		playerModel.InventoryModel.OnCashCountChanged += OnCashChanged;

		OnCashChanged();
		OnTickChanged();
		OnDaysChanged();
	}

	protected override void OnDeactivate()
	{
		playerModel.InventoryModel.OnCashCountChanged -= OnCashChanged;

		runModel.LevelModel.DayChanged -= OnDaysChanged;
		runModel.LevelModel.TickChanged -= OnTickChanged;
	}

	private void OnTickChanged()
	{
		RotateSun();

		_context.SetTicksText("sessions_count", (runModel.LevelModel.Ticks - runModel.LevelModel.Tick).ToString());
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
		_context.SetDaysText("days_count", (runModel.LevelModel.Day + 1).ToString(), runModel.LevelModel.Days.ToString());
	}

	private void OnCashChanged()
	{
		_context.SetCashProgress("game_progress", $"{playerModel.InventoryModel.CashCount}", $"{runModel.LevelModel.CashGoal}");
	}
}
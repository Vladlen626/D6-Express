using PlatformCore.Core;
using PlatformCore.Services.UI;
using UnityEngine;

public class LevelViewController : BaseContextController<UILevelView>
{
	private readonly PlayerModel playerModel;
	private readonly Run run;
	private readonly Light sun;

	public LevelViewController(IUIService uiService, PlayerModel playerModel, Run run, Light sun) : base(uiService)
	{
		this.playerModel = playerModel;
		this.run = run;
		this.sun = sun;
	}

	protected override void OnActivate()
	{
		run.TickChanged += OnTickChanged;
		run.TicksPerDayChanged += OnTickChanged;

		run.DayChanged += OnDaysChanged;
		run.DaysPerLevelChanged += OnDaysChanged;
		
		playerModel.InventoryModel.OnCashCountChanged += OnCashChanged;
		run.NextTicketPriceChanged += OnCashChanged;

		OnCashChanged();
		OnTickChanged();
		OnDaysChanged();
	}

	protected override void OnDeactivate()
	{
		playerModel.InventoryModel.OnCashCountChanged -= OnCashChanged;

		run.DayChanged -= OnDaysChanged;
		run.TickChanged -= OnTickChanged;
	}

	private void OnTickChanged()
	{
		RotateSun();

		_context.SetTicksText("ticks_progress", (run.TicksPerDay - run.Tick).ToString());
	}

	private void RotateSun()
	{
		var ratio = run.TicksPerDay > 0 ? (run.Tick / run.TicksPerDay) : 0;
		var currentTickRatio = _context.RatioModifier.Evaluate(ratio);

		sun.transform.rotation = Quaternion.Euler(currentTickRatio * 360f - 90f, 170f, 0f);
		sun.color = _context.LightColor.Evaluate(currentTickRatio);
		sun.intensity = _context.LightIntensity.Evaluate(currentTickRatio);
	}

	private void OnDaysChanged()
	{
		_context.SetDaysText("days_progress", (run.DaysPerLevel - run.Day).ToString());
	}

	private void OnCashChanged()
	{
		_context.SetCashProgress("cash_progress", $"{playerModel.InventoryModel.CashCount}", $"{run.NextTicketPrice}");
	}
}
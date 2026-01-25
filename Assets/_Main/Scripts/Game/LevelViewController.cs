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
		ChangeDayState();

		_context.SetTicksText("ticks_progress", (run.TicksPerDay - run.Tick).ToString());
	}

	private void ChangeDayState()
	{
		//Todo: добавить смену дня на ночь.
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
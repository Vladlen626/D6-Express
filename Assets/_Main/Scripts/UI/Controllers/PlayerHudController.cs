using PlatformCore.Core;
using PlatformCore.Services.UI;

namespace _Main.Scripts.UI
{
	public class PlayerHudController : BaseContextController<UIPlayerHud>
	{
		private readonly PlayerModel playerModel;
		private readonly D6Game game;
        private readonly Run run;

		public PlayerHudController(IUIService uiService, PlayerModel playerModel, D6Game game, Run run) : base(uiService)
		{
			this.playerModel = playerModel;
			this.game = game;
            this.run = run;
        }

		protected override void OnActivate()
		{
			base.OnActivate();
			playerModel.InventoryModel.OnCashCountChanged += OnCashCountChangedHandler;
			OnCashCountChangedHandler();

			game.LocationChanged += OnLocationChanged;

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

			game.LocationChanged -= OnLocationChanged;

			playerModel.InventoryModel.OnCashCountChanged -= OnCashCountChangedHandler;
			base.OnDeactivate();
		}

		private void OnCashCountChangedHandler()
		{
			var cashCountText = $"$: {playerModel.InventoryModel.CashCount}";
			_context.SetCashCountText(cashCountText);
		}

		private void OnTickChanged()
		{
			_context.SetTicksText("ticks_progress", (run.TicksPerDay - run.Tick).ToString());
		}

		private void OnDaysChanged()
		{
			_context.SetDaysText("days_progress", (run.DaysPerLevel - run.Day).ToString());
		}

		private void OnCashChanged()
		{
			_context.SetCashProgressText("cash_progress", $"{playerModel.InventoryModel.CashCount}", $"{run.NextTicketPrice}");
		}

		private void OnLocationChanged()
		{
			if (game.Location == Location.MAIN_MENU)
			{
				_context.Hide();
			}
			else
			{
				_context.Show();
			}
		}
	}
}
using PlatformCore.Core;
using PlatformCore.Services.UI;

namespace _Main.Scripts.UI
{
	public class PlayerHudController : BaseContextController<UIPlayerHud>
	{
		private readonly PlayerModel playerModel;
		private readonly D6Game game;
        private readonly Run run;
		private int _lastCash = int.MinValue;
		private int _lastTicksLeft = int.MinValue;
		private int _lastDaysLeft = int.MinValue;

		public PlayerHudController(IUIService uiService, PlayerModel playerModel, D6Game game, Run run) : base(uiService)
		{
			this.playerModel = playerModel;
			this.game = game;
            this.run = run;
        }

		protected override void OnActivate()
		{
			base.OnActivate();
			_lastCash = int.MinValue;
			_lastTicksLeft = int.MinValue;
			_lastDaysLeft = int.MinValue;

			playerModel.InventoryModel.OnCashCountChanged += OnCashCountChangedHandler;
			OnCashCountChangedHandler();

			game.LocationChanged += OnLocationChanged;

			run.TickChanged += OnTickChanged;
			run.TicksPerDayChanged += OnTickChanged;

			run.DayChanged += OnDaysChanged;
			run.DaysPerLevelChanged += OnDaysChanged;

			OnTickChanged();
			OnDaysChanged();
		}

		protected override void OnDeactivate()
		{
			run.DayChanged -= OnDaysChanged;
			run.TickChanged -= OnTickChanged;
			run.DaysPerLevelChanged -= OnDaysChanged;
			run.TicksPerDayChanged -= OnTickChanged;

			game.LocationChanged -= OnLocationChanged;

			playerModel.InventoryModel.OnCashCountChanged -= OnCashCountChangedHandler;
			base.OnDeactivate();
		}

		private void OnCashCountChangedHandler()
		{
			int cash = playerModel.InventoryModel.CashCount;
			if (_lastCash == cash)
			{
				return;
			}

			_lastCash = cash;
			_context.SetCashCountText(cash);
		}

		private void OnTickChanged()
		{
			int ticksLeft = run.TicksPerDay - run.Tick;
			if (_lastTicksLeft == ticksLeft)
			{
				return;
			}

			_lastTicksLeft = ticksLeft;
			_context.SetTicksText("ticks_progress", ticksLeft);
		}

		private void OnDaysChanged()
		{
			int daysLeft = run.DaysPerLevel - run.Day;
			if (_lastDaysLeft == daysLeft)
			{
				return;
			}

			_lastDaysLeft = daysLeft;
			_context.SetDaysText("days_progress", daysLeft);
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

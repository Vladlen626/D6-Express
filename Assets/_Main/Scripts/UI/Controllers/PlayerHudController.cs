using PlatformCore.Core;
using PlatformCore.Services.UI;

namespace _Main.Scripts.UI
{
	public class PlayerHudController : BaseContextController<UIPlayerHud>
	{
		private readonly PlayerModel playerModel;
		
		private InventoryModel inventoryModel => playerModel.InventoryModel;
		
		public PlayerHudController(IUIService uiService, PlayerModel playerModel) : base(uiService)
		{
			this.playerModel = playerModel;
		}

		protected override void OnActivate()
		{
			base.OnActivate();
			inventoryModel.OnCashCountChanged += OnCashCountChangedHandler;
			OnCashCountChangedHandler();
		}

		protected override void OnDeactivate()
		{
			inventoryModel.OnCashCountChanged -= OnCashCountChangedHandler;
			base.OnDeactivate();
		}

		private void OnCashCountChangedHandler()
		{
			var cashCountText = $"$: {inventoryModel.CashCount}";
			_context.SetCashCountText(cashCountText);
		}
	}
}
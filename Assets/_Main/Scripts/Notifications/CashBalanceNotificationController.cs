using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Audio;

public class CashBalanceNotificationController : IBaseController, IActivatable
{
	private readonly InventoryModel inventoryModel;
	private readonly IAudioService audioService;
	private readonly GlobalNotificationService notificationService;
	private int previousCash;

	public CashBalanceNotificationController(InventoryModel inventoryModel, IAudioService audioService, GlobalNotificationService notificationService)
	{
		this.inventoryModel = inventoryModel;
		this.audioService = audioService;
		this.notificationService = notificationService;
	}

	public void Activate()
	{
		if (inventoryModel == null)
		{
			return;
		}

		previousCash = inventoryModel.CashCount;
		inventoryModel.OnCashCountChanged += OnCashCountChangedHandler;
	}

	public void Deactivate()
	{
		if (inventoryModel == null)
		{
			return;
		}

		inventoryModel.OnCashCountChanged -= OnCashCountChangedHandler;
	}

	private void OnCashCountChangedHandler()
	{
		if (notificationService == null || inventoryModel == null)
		{
			return;
		}

		var currentCash = inventoryModel.CashCount;
		var delta = currentCash - previousCash;
		previousCash = currentCash;

		if (delta == 0)
		{
			return;
		}
		
		audioService.PlaySound(SoundNames.SpendMoney);

		var isNegative = delta < 0;
		var absoluteDelta = isNegative ? -delta : delta;
		var deltaText = isNegative ? $"-{absoluteDelta}$" : $"+{absoluteDelta}$";
		notificationService.EnqueueToastRaw(deltaText, isNegative);
	}
}

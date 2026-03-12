using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Connects a DiceItemView with a modifier item and feeds state changes both ways.
	/// Register this controller in the lifecycle to hook/unhook automatically.
	/// </summary>
	public class ModifierItemController : IBaseController, IActivatable
	{
		private readonly IModifierItem item;
		private readonly DiceItemView view;
		private readonly DiceGameModel diceGameModel;
		private readonly GlobalNotificationService notificationService;

		public ModifierItemController(
			IModifierItem item,
			DiceItemView view,
			DiceGameModel diceGameModel,
			GlobalNotificationService notificationService)
		{
			this.item = item;
			this.view = view;
			this.diceGameModel = diceGameModel;
			this.notificationService = notificationService;
		}

		public void Activate()
		{
			item.AttachView(view);
			view.OnClicked.AddListener(OnViewClicked);
		}

		public void Deactivate()
		{
			view.OnClicked.RemoveListener(OnViewClicked);
			item.DetachView();
		}

		private void OnViewClicked()
		{
			if (item.State == DiceItemState.Ready &&
			    diceGameModel != null &&
			    !item.IsActivationAllowed(diceGameModel.DiceGameState))
			{
				if (!string.IsNullOrWhiteSpace(item.InvalidActivationNotificationKey))
				{
					notificationService?.ShowToastImmediate(item.InvalidActivationNotificationKey, true);
				}

				return;
			}

			item.TryHandleClick();
		}
	}
}

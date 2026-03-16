using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Connects an ItemView with a modifier item and feeds state changes both ways.
	/// Register this controller in the lifecycle to hook/unhook automatically.
	/// </summary>
	public class ModifierItemController : IBaseController, IActivatable
	{
		private readonly IModifierItem item;
		private readonly ItemView view;
		private readonly DiceGameModel diceGameModel;
		private readonly GlobalNotificationService notificationService;

		public ModifierItemController(
			IModifierItem item,
			ItemView view,
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
			item.OnChanged += OnItemChanged;

			if (diceGameModel != null)
			{
				diceGameModel.OnDiceGameStateChanged += OnDiceGameStateChanged;
			}

			RefreshPhaseDisabledVisual();
		}

		public void Deactivate()
		{
			if (diceGameModel != null)
			{
				diceGameModel.OnDiceGameStateChanged -= OnDiceGameStateChanged;
			}

			item.OnChanged -= OnItemChanged;
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

		private void OnDiceGameStateChanged()
		{
			RefreshPhaseDisabledVisual();
		}

		private void OnItemChanged(IModifierItem _)
		{
			RefreshPhaseDisabledVisual();
		}

		private void RefreshPhaseDisabledVisual()
		{
			if (!view || item == null || diceGameModel == null)
			{
				return;
			}

			var isPhaseDisabled = item.ActivationType == DiceItemActivationType.ClickToActivate &&
			                      item.State == DiceItemState.Ready &&
			                      !item.IsActivationAllowed(diceGameModel.DiceGameState);
			view.SetPhaseDisabled(isPhaseDisabled);
		}
	}
}
